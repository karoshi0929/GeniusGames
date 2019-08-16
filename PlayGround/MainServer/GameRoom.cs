using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DataHandler;

namespace MainServer
{
    public class GameRoom
    {
        const short CARDMINNUM = 1;
        const short CARDMAXNUM = 20;
        IndianPokerServer SendGameMessage = new IndianPokerServer();

        int gameRoomNumber = 0;

        GamePlayer player1;
        GamePlayer player2;

        int currentTurnPlayer;
        short[] card = new short[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10};


        public void EnterGameRoom(ClientInfo user1, ClientInfo user2,int RoomNumber)
        {
            this.gameRoomNumber = RoomNumber;

            player1 = new GamePlayer(user1, 1, RoomNumber);
            player2 = new GamePlayer(user2, 2, RoomNumber);

            user1.EnterClientGameRoom(player1, this);
            user2.EnterClientGameRoom(player2, this);
            
            GameStart();
        }

        public void GameStart()
        {
            Random random = new Random();

            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.startGame = 0x01;
            //gamePacket.playerTurn = player1.PlyaerIndex;
            gamePacket.card = (short)random.Next(CARDMINNUM, CARDMAXNUM);

            //Delegate사용해서 Send해야됨
            //SendGameMessage.SendMessage(Header.Game, gamePacket, player1.owner.ClientSocket);
            //SendGameMessage.SendMessage(Header.Game, gamePacket, player2.owner.ClientSocket);
        }

        public void RequestBetting()
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
            gameRoom.EnterGameRoom(Player1, Player2, gameRoomList.Count);
            gameRoomList.Add(gameRoom);
        }

        public void DestroyGameRoom(GameRoom gameRoom)
        {
            gameRoomList.Remove(gameRoom);
        }
    }
}
