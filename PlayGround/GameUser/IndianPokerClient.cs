using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DataHandler;


namespace TCPcommunication
{
    public class IndianPokerClient
    {
        private TcpClient ClientSocket;
        private string strServerAddress;
        private int Port;
        private short ID;

        public PacketDefine packetDefine = new PacketDefine();
        private NetworkStream stream = default(NetworkStream);

        public IndianPokerClient(string address, int port, short id)
        {
            this.ClientSocket = new TcpClient();
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
                this.ClientSocket.Connect(this.strServerAddress, this.Port);
                this.stream = this.ClientSocket.GetStream();

                
                //SendMessage(Header.Login, LoginData);
            }
            catch
            {
                Console.WriteLine("서버가 실행중이 아님.");
                return false;
            }
            return true;
        }

        //매치 요청
        public bool RequestMatch()
        {
            if(SendMessage2(this.ID, (int)Matching.StartMatching))
            {
                Console.WriteLine("매칭 요청");
                return true;
            }
            return false;
        }

        //메시지 전달
        private bool SendMessage2(short id, int value)
        {
            MatchingData message = new MatchingData();
            message.GameID = (byte)KindOfGame.IndianPokser;
            message.Ack = 1;
            message.matchingMsg = (byte)Matching.StartMatching;

            //byte[] sendMessage = packetDefine.MakePacket(message);
            //stream.Write(sendMessage, 0, sendMessage.Length);
            return true;
        }

        public bool SendMessage(Header header, object data)
        {
            byte[] sendData = PacketDefine.MakePacket(header, data);
            stream.Write(sendData, 0, sendData.Length);

            return true;
        }
    }

}
