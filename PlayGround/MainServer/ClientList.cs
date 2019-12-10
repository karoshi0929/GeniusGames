using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DataHandler;
using System.Collections.ObjectModel;

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

        public void RemoveClient(string clientID)
        {
            ClientInfoDic[clientID].ClientSocket.Disconnect(true);
            ClientInfoDic[clientID].ClientSocket.Close();
            ClientInfoDic.Remove(clientID);
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

        public void EnterClientGameRoom(GamePlayer player,GameRoom CurrentGameRoom)
        {
            this.gamePlayer = player;
            this.gameRoom = CurrentGameRoom;
        }
    }

    public class GamePlayer
    {
        //GameStart()함수에서 Send할때 사용하기때문에 public이지만, private로 지정해야함. 
        public ClientInfo owner;
        public bool isReadyForGame = false;
        private int playerInRoomNumber;
        private short playerIndex;
        private short playerTurn;

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

        public short PlayerTurn
        {
            get
            {
                return playerTurn;
            }
            set
            {
                playerTurn = value;
            }
        }

        public int PlayerInRoomNumber
        {
            get { return playerInRoomNumber; }
            set { playerInRoomNumber = value; }
        }


        public GamePlayer(ClientInfo user, short playerNumber, int gameRoomNumber, short currentTurn)
        {
            this.PlayerInRoomNumber = gameRoomNumber;
            this.owner = user;
            this.PlayerIndex = playerNumber;
            this.PlayerMoney = 1000;
            this.playerTurn = currentTurn;
        }
    }
}
