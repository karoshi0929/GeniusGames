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
        
        //클라이언트 리스트 저장 컬렉션
        public List<Socket> ClientList = new List<Socket>();
        //Dictionary<string, Socket> ClientInfoDic = new Dictionary<string, Socket>();

        const int bufferSize = 1024;
        byte[] ReceiveBuffer = new byte[bufferSize];

        private Socket ServerSocket;
        private Socket ClientSocket;

        private object Lock = new object();

        public void OpenIndianPokerServer()
        {
            try
            {
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10000);
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

            //string strWelcome = "서버에 접속 하였습니다.";
            //byte[] sendMessage = Encoding.UTF8.GetBytes(strWelcome);
            //ClientSocket.BeginSend(sendMessage, 0, sendMessage.Length, SocketFlags.None, new AsyncCallback(SendMessage2), ClientSocket);

            printText("클라이언트" + ClientSocket.RemoteEndPoint.ToString() + "입장하였습니다.");


            AsyncObject ao = new AsyncObject(1024);
            ao.WorkingSocket = ClientSocket;
            ClientSocket.BeginReceive(ao.Buffer, 0, ao.BufferSize, 0, new AsyncCallback(ReceiveMessage), ao);
        }

        public void SendMessage(Header header, object data, Socket EndPointClientSocket)
        {
            byte[] sendData = PacketDefine.MakePacket(header, data);
            //ServerSocket.SendTo(sendData, EndPointClientSocket);
            //ServerSocket.SendTo(sendData, EndPointClientSocket.RemoteEndPoint);

            EndPointClientSocket.BeginSend(sendData, 0, sendData.Length, SocketFlags.None, new AsyncCallback(SendMessage2), EndPointClientSocket);
        }

        private void SendMessage2(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            int sent = client.EndSend(iar);
        }

        private void ReceiveMessage(IAsyncResult iar)
        {
            //받은 메세지를 처리하기전에 다른 메세지가 들어온다면 에러가 나기때문에
            //동기 코드가 필요할꺼같다.
            
            AsyncObject client = (AsyncObject)iar.AsyncState;
            int recv = client.WorkingSocket.EndReceive(iar);

            if (recv > 0)
            {
                //메세지를 받았을 경우
                PacketParser.PacketParsing(client.Buffer, client.WorkingSocket);

                client.ClearBuffer();
                ClientSocket.BeginReceive(client.Buffer, 0, client.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
            }
            else
            {
                //메세지를 못받았을 경우
                client.WorkingSocket.Close();
                return;
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
