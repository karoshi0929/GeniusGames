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
        private Socket clientSocket;
        
        private bool isLogin;
        private bool isPlayGame;

        private string clientID;

        public GamePlayer gamePlayer;
        public GameRoom gameRoom;

        public bool isReadyForGame;

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

        public void EnterClientGameRoom(GamePlayer player,GameRoom CureentGameRoom)
        {
            this.gamePlayer = player;
            this.gameRoom = CureentGameRoom;
        }
    }

    public class GamePlayer
    {
        //GameStart()함수에서 Send할때 사용하기때문에 public이지만, private로 지정해야함. 
        public ClientInfo owner;
        public bool isReadyForGame = false;
        private int playerInRoomNumber;
        private short playerIndex;

        public short PlayerIndex
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

        public int PlayerMoney
        {
            get;
            set;
        }


        public GamePlayer(ClientInfo user, short playerNumber, int gameRoomNumber, int playerMoney = 0)
        {
            this.playerInRoomNumber = gameRoomNumber;
            this.owner = user;
            this.PlayerIndex = playerNumber;
            this.PlayerMoney = 1000;
        }
    }
}
