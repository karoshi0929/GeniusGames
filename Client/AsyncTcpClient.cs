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

namespace ACTD.NET
{
    public class AsyncTcpClient : ThreadManaged, INetworkClient
    {
        private TcpClient m_client = new TcpClient();
        private ClientOption m_clientOps = null;
        private Object m_generalLock = new Object();
        private Object m_sendLock = new Object();
        private Object m_sendQueueLock = new Object();
        private Queue<PacketTransporter> m_sendQueue = new Queue<PacketTransporter>();

        private INetworkClientCallback m_callBackObj = null;
        private String m_hostName;
        private String m_port;
        private bool m_noDelay;
        private int m_waitTimeInMilliSec;

        private EventManaged m_timeOutEvent = new EventManaged(false, EventResetMode.AutoReset);
        private EventManaged m_sendEvent = new EventManaged();
        private Packet m_headerPacket = new Packet(null, 7);
        //private Packet m_headerPacket = null;
        private bool m_isConnected = false;
        private ServerType m_serverType;
        private object m_serverVersion;
        private int m_headerLength = 0;
        public AsyncTcpClient() : base()
        {
        }
        public INetworkClientCallback ClientCallBackOBJ
        {
            get { return m_callBackObj; }
            set { m_callBackObj = value; }
        }

        public AsyncTcpClient(AsyncTcpClient b) : base(b)
        {
            m_clientOps = b.m_clientOps;
        }

        ~AsyncTcpClient()
        {
            if (IsConnectionAlive())
                Disconnect();
        }

        public String GetHostName()
        {
            lock (m_generalLock)
            {
                return m_hostName;
            }
        }

        public String GetPort()
        {
            lock (m_generalLock)
            {
                return m_port;
            }
        }
        public Packet GetHeaderPacket()
        {
            return m_headerPacket;
        }

        public void SetServerType(ServerType type, object Version)
        {
            m_serverType = type;
            m_serverVersion = Version;
            if (m_serverType == ServerType.RADAR)
            {
                m_headerLength = Marshal.SizeOf(typeof(BLOCK_HEADER_T));
                m_headerPacket = new Packet(null, m_headerLength);
            }
            else if (m_serverType == ServerType.EOIR || m_serverType == ServerType.EOIRByPassServer)
            {
                m_headerLength = Marshal.SizeOf(typeof(EOIR_Header));
                m_headerPacket = new Packet(null, m_headerLength);
            }
        }

        public ServerType GetServerType()
        {
            return m_serverType;
        }
        public object GetServerVersion()
        {
            return m_serverVersion;
        }
        private class CallbackException : Exception
        {
            public CallbackException()
                : base()
            {

            }
            public CallbackException(String message)
                : base(message)
            {

            }
        }

        protected override void execute()
        {
            ConnectStatus status = ConnectStatus.SUCCESS;
            try
            {
                lock (m_generalLock)
                {
                    if (IsConnectionAlive())
                    {
                        status = ConnectStatus.FAIL_ALREADY_CONNECTED;
                        throw new CallbackException();
                    }

                    m_callBackObj = m_clientOps.callBackObj;
                    m_hostName = m_clientOps.hostName;
                    m_port = m_clientOps.port;
                    m_noDelay = m_clientOps.noDelay;
                    m_waitTimeInMilliSec = m_clientOps.waitTimeInMilliSec;

                    if (m_hostName == null || m_hostName.Length == 0)
                    {
                        m_hostName = ServerConf.DEFAULT_HOSTNAME;
                    }

                    if (m_port == null || m_port.Length == 0)
                    {
                        m_port = ServerConf.DEFAULT_PORT;
                    }

                    m_client.NoDelay = m_noDelay;

                    m_client.Client.BeginConnect(m_hostName, Convert.ToInt32(m_port), new AsyncCallback(onConnected), this);
                    if (m_timeOutEvent.WaitForEvent(m_waitTimeInMilliSec))
                    {
                        if (!m_client.Connected)
                        {
                            status = ConnectStatus.FAIL_SOCKET_ERROR;
                            throw new CallbackException();
                        }
                        m_isConnected = true;
                        if (m_callBackObj != null)
                        {
                            Thread t = new Thread(delegate ()
                            {
                                m_callBackObj.OnConnected(this, ConnectStatus.SUCCESS);
                            });
                            t.Start();
                        }
                    }
                    else
                    {
                        m_client.Close();
                        status = ConnectStatus.FAIL_TIME_OUT;
                        throw new CallbackException();
                    }
                }
            }
            catch (CallbackException)
            {
                if (m_callBackObj != null)
                {
                    Thread t = new Thread(delegate ()
                    {
                        m_callBackObj.OnConnected(this, status);
                    });
                    t.Start();

                }
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                if (m_callBackObj != null)
                {
                    Thread t = new Thread(delegate ()
                    {
                        m_callBackObj.OnConnected(this, ConnectStatus.FAIL_SOCKET_ERROR);
                    });
                    t.Start();

                }
                return;
            }

            startReceive();

        }

        public void Connect(ClientOption ops)
        {
            lock (m_generalLock)
            {
                if (IsConnectionAlive())
                    return;
            }

            if (ops == null)
            {
                ops = ClientOption.defaultClientOps;
            }

            if (ops.callBackObj == null)
            {
                throw new NullReferenceException("callBackObj is null!");
            }

            lock (m_generalLock)
            {
                m_clientOps = ops;
            }

            Start();
        }

        private void onConnected(IAsyncResult result)
        {
            AsyncTcpClient tcpclient = result.AsyncState as AsyncTcpClient;

            try { tcpclient.m_client.Client.EndConnect(result); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                tcpclient.m_timeOutEvent.SetEvent();
                return;
            }
            tcpclient.m_timeOutEvent.SetEvent();

            return;

        }

        public void Disconnect()
        {
            lock (m_generalLock)
            {
                if (!IsConnectionAlive())
                    return;
                m_client.Close();
                m_isConnected = false;
            }

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

        public bool IsConnectionAlive()
        {
            return m_isConnected;
        }

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
                // 전송 예외 처리를 위한 설정
                int headSize = packet.GetPacketByteSize();
                Packet sendSizePacket = new Packet(null, headSize, false);
                PacketTransporter transport = new PacketTransporter(PacketType.DATA, sendSizePacket, 0, headSize, this, -1, packet);
                // 전송 패킷 설정
                sendSizePacket.SetPacket(packet.GetPacket(), 0);
                //while (true)
                //{
                    if (m_sendEvent.TryLock())
                    {
                        try
                        {
                            m_client.Client.BeginSend(sendSizePacket.GetPacket(), 0, headSize, SocketFlags.None, new AsyncCallback(onSent), transport);
                            //break;
                        }
                        catch (Exception ex)
                        {
                            // 예외 발생시 콜백 수신되는 transport로 재전송 처리
                            Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                            if (m_callBackObj != null)
                                m_callBackObj.OnSent(this, SendStatus.FAIL_SOCKET_ERROR);
                            Disconnect();
                            return;
                        }

                    }
                //}
                else
                {
                    lock (m_sendQueueLock)
                    {
                        m_sendQueue.Enqueue(transport);
                    }
                }
            }
        }

        private enum PacketType
        {
            // Header
            HEADER = 0,
            // Payload
            DATA
        }

        private class PacketTransporter
        {
            public Packet m_packet;
            public Packet m_dataPacket;
            public int m_offset;
            public int m_size;
            public AsyncTcpClient m_TcpClient;
            public PacketType m_packetType;
            public INetworkClientCallback m_callBackObj;
            public Packet HeaderPacket { get; set; }
            public int m_DataType;

            public PacketTransporter(PacketType packetType, Packet packet, int offset, int size, AsyncTcpClient tcpClient, int dataType = -1, Packet dataPacket = null)
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

        private void startReceive()
        {
            // TCP/IP 패킷의 헤더부터 먼저 수신
            PacketTransporter transport = new PacketTransporter(PacketType.HEADER, m_headerPacket, 0, m_headerLength, this);
            try { m_client.Client.BeginReceive(m_headerPacket.GetPacket(), 0, m_headerLength, SocketFlags.None, new AsyncCallback(onReceived), transport); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                Disconnect(); return;
            }
        }

        private void onReceived(IAsyncResult result)
        {
            PacketTransporter transport = result.AsyncState as PacketTransporter;
            Socket socket = transport.m_TcpClient.m_client.Client;
            int headerLength = transport.m_TcpClient.m_headerLength;
            
            //소켓 통신에서 읽은 데이터 사이즈
            int readSize = 0;
            try { readSize = socket.EndReceive(result); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                transport.m_TcpClient.Disconnect(); return;
            }

            //읽은 데이터 사이즈가 0일 경우 통신 절단
            if (readSize == 0)
            {
                transport.m_TcpClient.Disconnect();
                return;
            }

            // True : 읽어야 할 데이터보다 적게 들어온 경우 더 받기 위해 Receive , False : 파싱 할 데이터가 모드 들어온 경우 HEADER 또는 DATA
            if (readSize < transport.m_size)
            {
                transport.m_offset = transport.m_offset + readSize;
                transport.m_size = transport.m_size - readSize;

                try { socket.BeginReceive(transport.m_packet.GetPacket(), transport.m_offset, transport.m_size, SocketFlags.None, new AsyncCallback(onReceived), transport); }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                    transport.m_TcpClient.Disconnect(); return;
                }
            }
            else
            {
                //True : 해더, False : 데이터
                if (transport.m_packetType == PacketType.HEADER)
                {
                    //해더 바이트
                    byte[] header = transport.m_packet.GetPacket();
                    //해더안의 데이터 길이를 알기 위한 변수
                    int DataLength = 0;
                    //체크 썸을 확인 하는 변수
                    bool isCheckSum = false;
                    //블록 타입
                    int iBlockType = 0;
                    //SetServerType을 이용한 서버 타입 Connect 하기 전에 설정된 타입값
                    
                    switch (m_serverType)
                    {
                        //페이로드 길이, 블록 타입, 체크 썸 결과를 반환한다.
                        case ServerType.RADAR:
                            isCheckSum = Radar_Packet_Receive(header, ref DataLength, ref iBlockType);
                            break;
                        case ServerType.EOIR:
                        case ServerType.EOIRByPassServer:
                            isCheckSum = EoIr_Packet_Receive(header, ref DataLength, ref iBlockType);
                            break;
                    }
                    //True : 완전한 헤더 데이터, False : 잘못된 헤더 데이터
                    if (isCheckSum && DataLength > 0)
                    {
                        //페이로드 수신 대기
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
                        //수신된 헤더 데이터 콜백 리시브
                        //transport.m_callBackObj.OnReceived(transport.m_TcpClient, new Packet(header, header.Length), iBlockType);
                    }
                    else
                    {
                        //해더 데이터만 존재하는 데이터 블록 리시브 콜백
                        if (isCheckSum)
                        {
                            Packet packet = new Packet(header, header.Length);
                            packet.SetDateTime();
                            transport.m_callBackObj.OnReceived(transport.m_TcpClient, packet, iBlockType);
                        }
                        else { }

                        //헤더 데이터 재수신 대기                  
                        PacketTransporter headerTransport = new PacketTransporter(PacketType.HEADER, transport.m_TcpClient.m_headerPacket, 0, headerLength, transport.m_TcpClient);
                        try
                        {
                            socket.BeginReceive(transport.m_TcpClient.m_headerPacket.GetPacket(), 0, headerLength, SocketFlags.None, new AsyncCallback(onReceived), headerTransport);
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
                    //페이로드 리시브 콜백
                    transport.m_dataPacket = transport.m_packet;


                    //리시브 받은 현재 시간을 저장
                    transport.m_dataPacket.SetDateTime();
                    transport.m_callBackObj.OnReceived(transport.m_TcpClient, transport.m_dataPacket, transport.m_DataType);

                    //페이로드가 정상 수신되어 해더 데이터 재수신 대기
                    PacketTransporter headerTransport = new PacketTransporter(PacketType.HEADER, transport.m_TcpClient.m_headerPacket, 0, headerLength, transport.m_TcpClient);
                    try
                    {
                        socket.BeginReceive(headerTransport.m_packet.GetPacket(), 0, headerLength, SocketFlags.None, new AsyncCallback(onReceived), headerTransport);
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

        private void onSent(IAsyncResult result)
        {
            PacketTransporter transport = result.AsyncState as PacketTransporter;
            Socket socket = transport.m_TcpClient.m_client.Client;

            int sentSize = 0;
            try { sentSize = socket.EndSend(result); }
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
                try { socket.BeginSend(transport.m_packet.GetPacket(), transport.m_offset, transport.m_size, SocketFlags.None, new AsyncCallback(onSent), transport); }
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
                if (transport.m_packetType == PacketType.HEADER)
                {
                    transport.m_packet = transport.m_dataPacket;
                    transport.m_offset = 0;
                    transport.m_packetType = PacketType.DATA;
                    transport.m_size = transport.m_dataPacket.GetPacketByteSize();
                    try { socket.BeginSend(transport.m_packet.GetPacket(), 0, transport.m_size, SocketFlags.None, new AsyncCallback(onSent), transport); }
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
                        try { socket.BeginSend(delayedTransport.m_packet.GetPacket(), 0, delayedTransport.m_size, SocketFlags.None, new AsyncCallback(onSent), delayedTransport); }
                        //try { socket.BeginSend(delayedTransport.m_packet.GetPacket(), 0, transport.m_size, SocketFlags.None, new AsyncCallback(AsyncTcpClient.onSent), delayedTransport); }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message + " >" + ex.StackTrace);
                            transport.m_callBackObj.OnSent(transport.m_TcpClient, SendStatus.SUCCESS);
                            delayedTransport.m_TcpClient.Disconnect();
                            delayedTransport.m_callBackObj.OnSent(delayedTransport.m_TcpClient, SendStatus.FAIL_SOCKET_ERROR);
                            return;
                        }
                    }
                    else
                    {
                        transport.m_TcpClient.m_sendEvent.Unlock();
                    }

                    transport.m_callBackObj.OnSent(transport.m_TcpClient, SendStatus.SUCCESS);
                }
            }
        }
    }
}
