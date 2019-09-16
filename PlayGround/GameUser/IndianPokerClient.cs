using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DataHandler;
using System.Net;

namespace TCPcommunication
{
    public class IndianPokerClient
    {
        //private TcpClient ClientSocket;
        private Socket ClientSocket;
        private IPEndPoint ep;
        public AsyncObject ao = new AsyncObject(1024);
        private string strServerAddress;
        private int Port;
        private string ID;

        public PacketDefine packetDefine = new PacketDefine();
        //private NetworkStream stream = default(NetworkStream);

        public IndianPokerClient(string address, int port, string id)
        {
            this.ep = new IPEndPoint(IPAddress.Parse(address), port);
            this.ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            this.strServerAddress = address;
            this.Port = port;
            this.ID = id;
            //서버 관련된 정보 변수 추가

            //ConnectedServer();
        }
        public bool ConnectedServer()
        {
            try
            {
                this.ClientSocket.Connect(this.ep);
                this.ao.WorkingSocket = this.ClientSocket;
                this.ClientSocket.BeginReceive(ao.Buffer, 0, ao.BufferSize, 0, ReceiveMessage, ao);
            }
            catch
            {
                Console.WriteLine("서버가 실행중이 아님.");
                return false;
            }
            return true;
        }

        public bool SendMessage(Header header, object data, Socket EndPointServerSocket)
        {
            byte[] sendData = PacketDefine.MakePacket(header, data);

            if(!this.ClientSocket.IsBound)
            {
                return false;
            }
            EndPointServerSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(SendMessage2), EndPointServerSocket);
            //this.ClientSocket.Send(sendData);

            return true;
        }

        private void SendMessage2(IAsyncResult iar)
        {
            Socket server = (Socket)iar.AsyncState;
            int sent = server.EndSend(iar);
        }

        private void ReceiveMessage(IAsyncResult iar)
        {
            AsyncObject client = (AsyncObject)iar.AsyncState;

            int recv = client.WorkingSocket.EndReceive(iar);

            if (recv > 0)
            {
                //메세지를 받았을 경우
                PacketParser.PacketParsing(client.Buffer);

            }
            else
            {
                //메세지를 못받았을 경우
                client.WorkingSocket.Close();
                return;
            }
            client.ClearBuffer();
            client.WorkingSocket.BeginReceive(client.Buffer, 0, 1024, SocketFlags.None, ReceiveMessage, client);
        }

    }
    public class AsyncObject
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[BufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }
    }
}
