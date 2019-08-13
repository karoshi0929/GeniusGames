using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DataHandler;

namespace MainServer
{
    public class ClientManagement
    {
        //string = ClientID, ClientInfo Class
        public Dictionary<string, ClientInfo> ClientInfoDic = new Dictionary<string, ClientInfo>();

        //List<ClientInfo> ClientSocket;


        public bool AddClient(ClientInfo clientInfo)
        {
            ClientInfoDic.Add(clientInfo.ClientID, clientInfo);
            return true;
        }

        public bool AddClient(LoginPacket loginPacket, Socket ClientSocket)
        {
            ClientInfo clientInfo = new ClientInfo(loginPacket, ClientSocket);

            ClientInfoDic.Add(clientInfo.ClientID, clientInfo);
            return true;
        }
    }

    public class ClientInfo
    {
        Socket clientSocket;
        bool isLogin;
        bool isPlayGame;
        string clientID;

        public Socket ClientSocket
        {
            get
            {
                return clientSocket;
            }
            set
            {
                clientSocket = value;
            }
        }

        public string ClientID
        {
            get
            {
                return clientID;
            }
            set
            {
                clientID = value;
            }
        }
        
        public bool IsLogin
        {
            get
            {
                return isLogin;
            }
            set
            {
                isLogin = value;
            }
        }

        public bool IsPlayGame
        {
            get
            {
                return isPlayGame;
            }
            set
            {
                isPlayGame = value;
            }
        }

        public ClientInfo(LoginPacket loginPacket,Socket Client)
        {
            ClientSocket = Client;
            ClientID = loginPacket.clientID;
            IsLogin = loginPacket.isLogin;
            //IsPlayGame = loginPacket.
        }

        public ClientInfo(ClientInfo user)
        {
            
        }
    }

    public class GamePlayer
    {
        ClientInfo owner;
        short playerIndex;
        public short PlyaerIndex
        {
            get
            {
                return playerIndex;
            }
            set
            {
                playerIndex = value;
            }
        }
        //게임에 입장한 플레이어에게 필요한 요소
        public int money;
        public int card;

        public GamePlayer(ClientInfo user, short playerNumber)
        {
            this.owner = user;
            this.PlyaerIndex = playerNumber;
            this.money = 100;
            this.card = 0;
        }
    }
}
