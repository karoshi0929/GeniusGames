using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MainServer
{
    public class GameRoom
    {
        public GameRoom()
        {

        }
    }

    public class GameRoomManager
    {
        Dictionary<int, GameRoom> GameRoomDic = new Dictionary<int, GameRoom>();

        public GameRoomManager()
        {

        }
    }
}
