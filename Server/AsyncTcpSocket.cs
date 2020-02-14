using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Diagnostics;
using ACTD.NetDATA.RADAR;
using System.Runtime.InteropServices;
using ACTD.NetDATA.EOIR;
using ACTD.NET.Event;

namespace ACTD.NET
{
    public class AsyncTcpSocket : ThreadManaged, INetworkSocket, IDisposable
    {
        private TcpClient m_client = null;
        private INetworkServer m_server = null;
        private IPInfo m_ipInfo;
        private Object m_generalLock = new Object();
        private Object m_sendLock = new Object();
        private Object m_sendQueueLock = new Object();
        private Queue<PacketTransporter> m_sendQueue = new Queue<PacketTransporter>();
        private INetworkSocketCallback m_callBackObj = null;
        private EventManaged m_sendEvent = new EventManaged();
        //private Packet m_recvPacket = new Packet(null, 12);
        private Packet m_recvPacket;
        private Packet m_sendPacket = new Packet(null, 4, false);
        private bool m_isConnected = false;
        private ServerType m_serverType;
        private int m_headerLength = 0;

        public AsyncTcpSocket(TcpClient client, INetworkServer server)
            : base()
        {
            m_client = client;
            m_server = server;
            IPEndPoint remoteIpEndPoint = m_client.Client.RemoteEndPoint as IPEndPoint;
            IPEndPoint localIpEndPoint = m_client.Client.LocalEndPoint as IPEndPoint;
            if (remoteIpEndPoint != null)
            {
                String socketHostName = remoteIpEndPoint.Address.ToString();
                m_ipInfo = new IPInfo(socketHostName, remoteIpEndPoint, IPEndPointType.REMOTE);
            }
            else if (localIpEndPoint != null)
            {
                String socketHostName = localIpEndPoint.Address.ToString();
                m_ipInfo = new IPInfo(socketHostName, localIpEndPoint, IPEndPointType.LOCAL);
            }

        }

        ~AsyncTcpSocket()
        {
            Dispose(false);

            if (IsConnectionAlive())
            {
                Disconnect();
            }
        }
        public INetworkSocketCallback SocketCallBackOBJ
        {
            get { return m_callBackObj; }
            set { m_callBackObj = value; }
        }

        /// <summary>
        /// Get IP information
        /// </summary>
        /// <returns>IP information</returns>
        public IPInfo GetIPInfo()
        {
            return m_ipInfo;
        }
        public Packet GetHeaderPacket()
        {
            return m_recvPacket;
        }
        /// <summary>
        /// Get managing server
        /// </summary>
        /// <returns>managing server</returns>
        public INetworkServer GetServer()
        {
            return m_server;
        }

        public ServerType GetServerType()
        {
            return m_serverType;
        }
        /// <summary>
        /// Set socket callback interface
        /// </summary>
        /// <param name="callBackObj">callback object</param>
        public void SetSocketCallback(INetworkSocketCallback callBackObj)
        {
            m_callBackObj = callBackObj;
        }
        /// <summary>
        /// Return the socket callback object
        /// </summary>
        /// <returns>the socket callback object</returns>
        public INetworkSocketCallback GetSocketCallback()
        {
            return m_callBackObj;
        }
        /// <summary>
        /// Start the new connection, and inform the callback object, that the new connection is made
        /// </summary>
        protected override void execute()
        {
            lock (m_generalLock)
            {
                m_isConnected = true;
            }
            startReceive();
            if (m_callBackObj != null)
                m_callBackObj.OnNewConnection(this);
        }

        /// <summary>
        /// Disconnect the client socket
        /// </summary>
        public void Disconnect()
        {
            lock (m_generalLock)
            {
                if (!IsConnectionAlive())
                    return;
                m_client.Close();
                m_isConnected = false;
            }
            m_server.DetachClient(this);

            lock (m_sendQueueLock)
            {
                m_sendQueue.Clear();
            }
            if (m_callBackObj != null)
            {
                Thread t = new Thread(delegate ()
                {
                    m_callBackObj.OnDisconnect(this);
                });
                t.Start();
            }
        }

        /// <summary>
        /// Check if the connection is alive
        /// </summary>
        /// <returns>true if connection is alive, otherwise false</returns>
        public bool IsConnectionAlive()
        {
            return m_isConnected;
            // 	        try
            // 	        {
            // 	            return m_client.Connected;
            // 	        }
            // 	        catch (Exception ex)
            // 	        {
            // 	            Console.WriteLine(ex.Message + " >" + ex.StackTrace);
            // 	            return false;
            // 	        }
        }

        /// <summary>
        /// Send given packet to the client
        /// </summary>
        /// <param name="packet">the packet to send</param>
        public void Send(Packet packet)
        {
            if (!IsConnectionAlive())
            {
                if (m_callBackObj != null)
                {
                    Thread t = new Thread(delegate ()
                    {
                        m_callBackObj.OnSent(this, SendStatus.FAIL_NOT_CONNECTED);
                    });
                    t.Start();
                }
                return;
            }
            if (packet.GetPacketByteSize() <= 0)
            {
                if (m_callBackObj != null)
                {
                    Thread t = new Thread(delegate ()
                    {
                        m_callBackObj.OnSent(this, SendStatus.FAIL_INVALID_PACKET);
                    });
                    t.Start();
                }
                return;
            }

            lock (m_sendLock)
            {
                int packetSize = packet.GetPacketByteSize();
                PacketTransporter transport = new PacketTransporter(PacketType.DATA, m_sendPacket, 0, packetSize, this, -1, packet);
                //m_sendPacket.SetPacket(BitConverter.GetBytes(packet.GetPacketByteSize()), 0);
                m_sendPacket.SetPacket(packet.GetPacket(), 0);
                while (true)
                {
                    if (m_sendEvent.TryLock())
                    {
                        try { m_client.Client.BeginSend(m_sendPacket.GetPacket(), 0, packetSize, SocketFlags.None, new AsyncCallback(onSent), transport); break; }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                            Disconnect();
                            return;
                        }

                    }
                }
                //else
                //{
                //    lock (m_sendQueueLock)
                //    {
                //        m_sendQueue.Enqueue(transport);
                //    }
                //}
            }
        }

        /// <summary>
        /// Packet Transporter class
        /// </summary>
        private class PacketTransporter
        {
            /// <summary>
            /// packet to transport
            /// </summary>
            public Packet m_packet;

            /// <summary>
            /// data packet for send
            /// </summary>
            public Packet m_dataPacket;

            /// <summary>
            /// offset
            /// </summary>
            public int m_offset;

            /// <summary>
            /// packet size in byte
            /// </summary>
            public int m_size;

            /// <summary>
            /// client socket
            /// </summary>
            public AsyncTcpSocket m_TcpClient;

            /// <summary>
            /// packet type
            /// </summary>
            public PacketType m_packetType;

            /// <summary>
            /// callback object
            /// </summary>
            public INetworkSocketCallback m_callBackObj;

            public Packet HeaderPacket { get; set; }
            public int m_DataType;
            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="packetType">packet type</param>
            /// <param name="packet">packet</param>
            /// <param name="offset">offset</param>
            /// <param name="size">size of packet in byte</param>
            /// <param name="iocpTcpClient">client socket</param>
            /// <param name="dataPacket">data packet for send</param>
            public PacketTransporter(PacketType packetType, Packet packet, int offset, int size, AsyncTcpSocket tcpClient, int dataType = -1, Packet dataPacket = null)
            {
                m_packetType = packetType;
                m_packet = packet;
                m_offset = offset;
                m_size = size;
                m_TcpClient = tcpClient;
                m_dataPacket = dataPacket;
                m_DataType = dataType;
                m_callBackObj = tcpClient.m_callBackObj;
            }
        }
        /// <summary>
        /// Start to receive packet from the server
        /// </summary>
        private void startReceive()
        {
            m_serverType = m_server.GetServerType();
            switch (m_server.GetServerType())
            {
                case ServerType.RADAR:
                    m_headerLength = Marshal.SizeOf(typeof(BLOCK_HEADER_T));
                    m_recvPacket = new Packet(null, 7);
                    break;
                case ServerType.EOIR:
                case ServerType.EOIRByPassServer:         
                    m_headerLength = Marshal.SizeOf(typeof(EOIR_Header));
                    m_recvPacket = new Packet(null, 12);
                    break;
            }

            if (m_headerLength > 0)
            {
                // 수정 필요
                PacketTransporter transport = new PacketTransporter(PacketType.HEADER, m_recvPacket, 0, m_headerLength, this);
                try
                {
                    m_client.Client.BeginReceive(m_recvPacket.GetPacket(), 0, m_headerLength, SocketFlags.None, new AsyncCallback(onReceived), transport);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                    Disconnect();
                    return;
                }
            }
        }

        /// <summary>
        /// Receive callback function
        /// </summary>
        /// <param name="result">result</param>
        private void onReceived(IAsyncResult result)
        {
            PacketTransporter transport = result.AsyncState as PacketTransporter;
            Socket socket = transport.m_TcpClient.m_client.Client;
            int headerLength = transport.m_TcpClient.m_headerLength;

            int readSize = 0;
            try
            {
                readSize = socket.EndReceive(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                transport.m_TcpClient.Disconnect();
                return;
            }

            // 소켓이 끊겼을 때 클라이언트 disconnect
            if (readSize == 0)
            {
                transport.m_TcpClient.Disconnect();
                return;
            }

            if (readSize < transport.m_size)
            {
                transport.m_offset = transport.m_offset + readSize;
                transport.m_size = transport.m_size - readSize;
                try
                {
                    socket.BeginReceive(transport.m_packet.GetPacket(), transport.m_offset, transport.m_size, SocketFlags.None, new AsyncCallback(onReceived), transport);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                    transport.m_TcpClient.Disconnect();
                    return;
                }
            }
            else
            {
                // 수신 받은 패킷이 헤더일 경우 다시 데이터 수신 준비
                if (transport.m_packetType == PacketType.HEADER)
                {
                    //해더 바이트
                    byte[] header = transport.m_packet.GetPacket();
                    //해더 안의 데이터 길이를 알기 위한 변수
                    int DataLength = 0;
                    //체크 썸을 확인 하는 변수
                    bool isCheckSum = false;
                    //블록 타입
                    int iBlockType = 0;

                    switch (m_serverType)
                    {
                        case ServerType.RADAR:
                            //Little Endian
                            isCheckSum = Radar_Packet_Receive(header, ref DataLength, ref iBlockType);
                            break;
                        case ServerType.EOIR:
                        case ServerType.EOIRByPassServer:
                            isCheckSum = EoIr_Packet_Receive(header, ref DataLength, ref iBlockType);
                            break;
                    }
                    if (isCheckSum && DataLength > 0)
                    {
                        //transport.m_callBackObj.OnReceived(transport.m_TcpClient, transport.m_packetType, new Packet(header, header.Length));

                        Packet recvPacket = new Packet(null, DataLength);
                        PacketTransporter dataTransport = new PacketTransporter(PacketType.DATA, recvPacket, 0, DataLength, transport.m_TcpClient);
                        dataTransport.HeaderPacket = transport.m_packet;
                        dataTransport.m_DataType = iBlockType;

                        try
                        {
                            socket.BeginReceive(recvPacket.GetPacket(), 0, DataLength, SocketFlags.None, new AsyncCallback(onReceived), dataTransport);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                            transport.m_TcpClient.Disconnect();
                            return;
                        }
                    }
                    else
                    {
                        // 헤더 데이터만 존재하는 데이터 블록 리시브 콜백
                        if (isCheckSum)
                        {
                            Packet packet = new Packet(header, header.Length);
                            packet.SetDateTime();
                            transport.m_callBackObj.OnReceived(transport.m_TcpClient, packet, iBlockType);
                        }
                        else
                        {

                        }
                        //Packet recvPacket = new Packet(header, headerLength);

                        //헤더 데이터 재수신 대기                  
                        PacketTransporter headerTransport = new PacketTransporter(PacketType.HEADER, transport.m_TcpClient.m_recvPacket, 0, headerLength, transport.m_TcpClient);
                        try
                        {
                            socket.BeginReceive(transport.m_TcpClient.m_recvPacket.GetPacket(), 0, headerLength, SocketFlags.None, new AsyncCallback(onReceived), headerTransport);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                            transport.m_TcpClient.Disconnect();
                            return;
                        }

                    }
                }
                else
                {

                    //if (m_serverType == ServerType.EOIR)
                    //{
                    //    transport.m_dataPacket = transport.m_packet;
                    //    transport.HeaderPacket.SetDateTime();
                    //    transport.m_dataPacket.SetDateTime();
                    //    transport.m_callBackObj.OnReceived(transport.m_TcpClient, transport.HeaderPacket, transport.m_dataPacket);

                    //}
                    //else
                    //{
                    //페이로드 리시브 콜백
                    transport.m_dataPacket = transport.m_packet;
                    //리시브 받은 현재 시간을 저장
                    transport.m_dataPacket.SetDateTime();
                    transport.m_callBackObj.OnReceived(transport.m_TcpClient, transport.m_dataPacket, transport.m_DataType);
                    //}


                    PacketTransporter sizeTransport = new PacketTransporter(PacketType.HEADER, transport.m_TcpClient.m_recvPacket, 0, headerLength, transport.m_TcpClient);
                    try
                    {
                        socket.BeginReceive(sizeTransport.m_packet.GetPacket(), 0, headerLength, SocketFlags.None, new AsyncCallback(onReceived), sizeTransport);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                        transport.m_TcpClient.Disconnect();
                        return;
                    }
                }
            }
        }


        private bool Radar_Packet_Receive(byte[] receivedHeader, ref int dataLength, ref int blockType)
        {
            //해더를 파싱하여 데이터 길이, 블록 타입을 추출한다.
            BLOCK_HEADER_T Header = NetDATA.RADAR.PacketParser.BlockHeaderSerialize(receivedHeader);
            dataLength = Header.DataLength;
            blockType = Header.BlockType;
            //체크섬 결과가 True 이면서 데이터 길이가 0보다 클때 TRUE 리턴 데이터 길이가 0보다 작을 경우 다시 해더를 받고 타입에 따라 처리
            if (Header.HeaderChecksum == NetDATA.RADAR.PacketParser.checkSumValue(Header)) return true;
            else return false;
        }
        private bool EoIr_Packet_Receive(byte[] receivedHeader, ref int dataLength, ref int blockType)
        {
            EOIR_Header bHeader = NetDATA.EOIR.PacketParser.BlockHeaderSerialize(receivedHeader);
            dataLength = unchecked((int)bHeader.DataLength);
            blockType = unchecked((int)bHeader.MessageID);
            //obj_Header = bHeader;
            return true;
        }

        /// <summary>
        /// Send callback function
        /// </summary>
        /// <param name="result">result</param>
        private void onSent(IAsyncResult result)
        {
            PacketTransporter transport = result.AsyncState as PacketTransporter;
            Socket socket = transport.m_TcpClient.m_client.Client;
            int sentSize = 0;

            try
            {
                sentSize = socket.EndSend(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                transport.m_TcpClient.Disconnect();
                transport.m_callBackObj.OnSent(transport.m_TcpClient, SendStatus.FAIL_SOCKET_ERROR);
                return;
            }

            if (sentSize == 0)
            {
                transport.m_TcpClient.Disconnect();
                transport.m_callBackObj.OnSent(transport.m_TcpClient, SendStatus.FAIL_CONNECTION_CLOSING);
                return;
            }

            if (sentSize < transport.m_size)
            {
                transport.m_offset = transport.m_offset + sentSize;
                transport.m_size = transport.m_size - sentSize;

                try
                {
                    socket.BeginSend(transport.m_packet.GetPacket(), transport.m_offset, transport.m_size, SocketFlags.None, new AsyncCallback(onSent), transport);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                    transport.m_TcpClient.Disconnect();
                    transport.m_callBackObj.OnSent(transport.m_TcpClient, SendStatus.FAIL_SOCKET_ERROR);
                    return;
                }
            }
            else
            {
                #region Not Used (차후 수정 필요)

                if (transport.m_packetType == PacketType.HEADER)
                {
                    transport.m_packet = transport.m_dataPacket;
                    transport.m_offset = 0;
                    transport.m_packetType = PacketType.DATA;
                    transport.m_size = transport.m_dataPacket.GetPacketByteSize();

                    try
                    {
                        socket.BeginSend(transport.m_packet.GetPacket(), 0, transport.m_size, SocketFlags.None, new AsyncCallback(onSent), transport);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                        transport.m_TcpClient.Disconnect();
                        transport.m_callBackObj.OnSent(transport.m_TcpClient, SendStatus.FAIL_SOCKET_ERROR);
                        return;
                    }
                }
                else
                {
                    PacketTransporter delayedTransport = null;

                    lock (transport.m_TcpClient.m_sendQueueLock)
                    {
                        Queue<PacketTransporter> sendQueue = transport.m_TcpClient.m_sendQueue;
                        if (sendQueue.Count > 0)
                        {
                            delayedTransport = sendQueue.Dequeue();
                        }
                    }

                    if (delayedTransport != null)
                    {
                        try
                        {
                            socket.BeginSend(delayedTransport.m_packet.GetPacket(), 0, delayedTransport.m_size, SocketFlags.None, new AsyncCallback(onSent), delayedTransport);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                            transport.m_TcpClient.Disconnect();
                            transport.m_callBackObj.OnSent(transport.m_TcpClient, SendStatus.FAIL_SOCKET_ERROR);
                            return;
                        }
                    }
                    else
                    {
                        transport.m_TcpClient.m_sendEvent.Unlock();
                    }

                    transport.m_callBackObj.OnSent(transport.m_TcpClient, SendStatus.SUCCESS);
                }

                #endregion
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            m_sendEvent.Dispose();
        }
    }
}
