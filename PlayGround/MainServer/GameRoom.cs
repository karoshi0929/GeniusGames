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
        enum Deal
        {
            //다이 삥 따당 체크 쿼터 하프
        }

        GamePlayer player1;
        GamePlayer player2;

        int currentTurnPlayer;
        int card;


        public void EnterGameRoom(ClientInfo user1, ClientInfo user2)
        {
            player1 = new GamePlayer(user1, 1);
            player2 = new GamePlayer(user2, 2);



            GameStart();
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
            GameRoom gameRoom = new GameRoom();
            gameRoom.EnterGameRoom(Player1, Player2);
            gameRoomList.Add(gameRoom);

            //gameRoom.GameStart();
        }

        public void RemoveGameRoom()
        {

        }
    }
}
