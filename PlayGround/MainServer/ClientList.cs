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
        public Dictionary<string, ClientInfo> ClientInfoDic = new Dictionary<string, ClientInfo>();

        List<ClientInfo> ClientSocket;
    }

    public class ClientInfo
    {
        Socket clientSocket;
        bool isLogin;
        bool isPlayGame;
        string clientID;

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

        public ClientInfo()
        {

        }
        
    }
}
