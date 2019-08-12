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
        ClientInfo client1;
        ClientInfo client2;

        public GameRoom(ClientInfo Player1, ClientInfo Player2)
        {
            client1 = new ClientInfo();
        }

        public void GameStart()
        {

        }
    }

    public class GameRoomManager
    {
        
        //Dictionary<int, GameRoom> GameRoomDic = new Dictionary<int, GameRoom>();
        List<GameRoom> gameRoomList = new List<GameRoom>();

        //public GameRoomManager(ClientInfo Player1, ClientInfo Player2)
        //{
        //    gameRoom = new GameRoom(Player1, Player2);
        //    gameRoomList.Add(gameRoom);
        //}

        public GameRoomManager()
        {
            
        }

        public void CreateGameRoom(ClientInfo Player1, ClientInfo Player2)
        {
            GameRoom gameRoom = new GameRoom(Player1, Player2);
            gameRoomList.Add(gameRoom);
        }
    }
}
