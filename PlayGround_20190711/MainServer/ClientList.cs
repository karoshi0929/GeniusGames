using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DataHandler;

namespace MainServer
{
    public class ClientManager
    {
        List<ClientInfo> ClientSocket;
        
        
    }

    public class ClientInfo
    {
        Socket s;
        bool is접속;
    }
}
