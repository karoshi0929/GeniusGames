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
        ClientInfo Player1;
        ClientInfo Player2;

        public GameRoom(ClientInfo user1, ClientInfo user2)
        {
            Player1 = new ClientInfo(user1);
            Player2 = new ClientInfo(user2);
        }

        public void EnterGameRoom()
        {

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
            gameRoom.EnterGameRoom();
            gameRoomList.Add(gameRoom);
        }

        public void RemoveGameRoom()
        {

        }
    }
}
