using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataHandler;

namespace MainServer
{
    public class IndianPokerServer
    {
        public delegate void PrintTextDelegate(string text);
        public PrintTextDelegate printText;

        const int bufferSize = 1024;
        byte[] ReceiveBuffer = new byte[bufferSize];

        private Socket ServerSocket;
        private Socket ClientSocket;

        private object recvLock = new object();
        private object sendLock = new object();

        public void OpenIndianPokerServer()
        {
            try
            {
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000);
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.2.42"), 10000);
                ServerSocket.Bind(endPoint);
                ServerSocket.Listen(10);
                ServerSocket.BeginAccept(new AsyncCallback(AcceptConnection), ServerSocket);
                printText("서버가 시작되었습니다.");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void AcceptConnection(IAsyncResult iar)
        {
            Socket oldServer = (Socket)iar.AsyncState;
            ClientSocket = oldServer.EndAccept(iar);
            
            ServerSocket.BeginAccept(new AsyncCallback(AcceptConnection), ServerSocket);

            printText("클라이언트" + ClientSocket.RemoteEndPoint.ToString() + "입장하였습니다.");

            AsyncObject ao = new AsyncObject(1024);
            ao.WorkingSocket = ClientSocket;
            ClientSocket.BeginReceive(ao.Buffer, 0, ao.BufferSize, 0, new AsyncCallback(ReceiveMessage), ao);
        }

        public void SendMessage(Header header, object data, Socket EndPointClientSocket)
        {
            byte[] sendData = PacketDefine.MakePacket(header, data);
            EndPointClientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(SendMessage2), EndPointClientSocket);
        }

        private void SendMessage2(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int sent = client.EndSend(iar);
        }

        private void ReceiveMessage(IAsyncResult iar)
        {
            AsyncObject client = new AsyncObject(1024);
            lock (recvLock)
            {
                client = (AsyncObject)iar.AsyncState;
                int recv = client.WorkingSocket.EndReceive(iar);

                if (recv > 0)
                {
                    //메세지를 받았을 경우
                    PacketParser.PacketParsing(client.Buffer, client.WorkingSocket);

                    client.ClearBuffer();
                    client.WorkingSocket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
                }
                else
                {
                    //메세지를 못받았을 경우
                    client.WorkingSocket.Close();
                    return;
                }
            }
        }
    }

    public class AsyncObject
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int buffersize)
        {
            BufferSize = buffersize;
            Buffer = new byte[BufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
        }
    }
}
