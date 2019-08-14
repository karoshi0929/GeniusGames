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
            catch(Exception ee)
            {
                printText("서버 열기 실패");
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

            
            ClientSocket.BeginReceive(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), ClientSocket);
            //ClientSocket.BeginReceive(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), ClientSocket);
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
            Socket client = (Socket)iar.AsyncState;
            
            int recv = client.EndReceive(iar);

            if(recv != 0)
            {
                //메세지를 받았을 경우
                byte[] recvData = ReceiveBuffer.ToArray();
                PacketParser.PacketParsing(recvData, client);

                //printText("클라이언트" + ClientSocket.RemoteEndPoint.ToString() + "매칭 요청하였습니다.");
            }
            else
            {
                //메세지를 못받았을 경우
            }
            ReceiveBuffer = new byte[bufferSize];
            ClientSocket.BeginReceive(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), client);
        }
    }
}
