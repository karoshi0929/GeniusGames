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

        PacketParser packetParser = new PacketParser();

        //클라이언트 리스트 저장 컬렉션
        public List<Socket> ClientList = new List<Socket>();
        //Dictionary<string, Socket> ClientInfoDic = new Dictionary<string, Socket>();

        const int bufferSize = 1024;
        byte[] ReceiveBuffer = new byte[bufferSize];

        private Socket ServerSocket;
        private Socket ClientSocket;

        public void OpenIndianPokerServer()
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.2.60"), 10000);
            ServerSocket.Bind(endPoint);
            ServerSocket.Listen(10);
            ServerSocket.BeginAccept(new AsyncCallback(AcceptConnection), ServerSocket);
        }

        private void AcceptConnection(IAsyncResult iar)
        {
            Socket oldServer = (Socket)iar.AsyncState;
            ClientSocket = oldServer.EndAccept(iar);

            ServerSocket.BeginAccept(new AsyncCallback(AcceptConnection), ServerSocket);

            string strWelcome = "서버에 접속 하였습니다.";
            byte[] sendMessage = Encoding.UTF8.GetBytes(strWelcome);
            ClientSocket.BeginSend(sendMessage, 0, sendMessage.Length, SocketFlags.None, new AsyncCallback(SendData), ClientSocket);

            //ClientList.Add(ClientSocket);
            printText("클라이언트" + ClientSocket.RemoteEndPoint.ToString() + "입장하였습니다.");
            
            
            ClientSocket.BeginReceive(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), ClientSocket);
            //ClientSocket.BeginReceive(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), ClientSocket);
        }

        private void SendData(IAsyncResult iar)
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
                byte[] recvData = ReceiveBuffer;
                packetParser.PacketParsing(recvData);

                printText("클라이언트" + ClientSocket.RemoteEndPoint.ToString() + "매칭 요청하였습니다.");
            }
            else
            {
                //메세지를 못받았을 경우
            }
            ClientSocket.BeginReceive(ReceiveBuffer, 0, ReceiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), ClientSocket);
        }
    }
}
