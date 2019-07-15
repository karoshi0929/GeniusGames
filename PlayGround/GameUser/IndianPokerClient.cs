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

        PacketDefine packetDefine = new PacketDefine();
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
            if(SendMessage(this.ID, (int)Matching.StartMatching))
            {
                Console.WriteLine("매칭 요청");
                return true;
            }
            return false;

        }

        //메시지 전달
        private bool SendMessage(short id, int value)
        {
            MatchingData message = new MatchingData();
            message.MessageID = (byte)MessageID.IndianPokser;
            message.Ack = 1;
            message.matchingMsg = (byte)Matching.StartMatching;

            byte[] sendMessage = packetDefine.MakePacket(message);
            stream.Write(sendMessage, 0, sendMessage.Length);
            return true;

            //string msg = "matching";
            //byte[] header = BitConverter.GetBytes(id);
            //byte[] body = BitConverter.GetBytes(value);
            //int length = header.Length + body.Length;
            //byte[] leng = BitConverter.GetBytes(length);
            //byte[] packet = new byte[length+sizeof(int)];

            //Array.Copy(header, 0, packet, 0, header.Length);
            //Array.Copy(leng, 0, packet, header.Length, leng.Length);
            //Array.Copy(body, 0, packet, header.Length + leng.Length, body.Length);

            //try
            //{
            //    stream.Write(packet, 0, packet.Length);
            //    stream.Flush();
            //    return true;
            //}
            //catch
            //{
            //    Console.WriteLine("메시지 전송 실패");
            //    return false;
            //}

        }
    }

}
