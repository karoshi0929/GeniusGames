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
        public delegate void DelegateSendGameStartMessage(Header header, HandleGamePacket gamePacket, ClientInfo clientSocket);
        public DelegateSendGameStartMessage SendGameStartMessage;

        public delegate void DelegateSendPokerMessage(Header header, IndianPokerGamePacket pokerPacket, ClientInfo clientSocket);
        public DelegateSendPokerMessage SendPokerGameMessage;

        const short CARDMINNUM = 1;
        const short CARDMAXNUM = 20;

        private int currentTurnPlayer;
        private int totalBettingMoney = 0;

        private short[] card = new short[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                                            1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

        public int gameRoomNumber = 0;

        public bool playGame = false;

        public GamePlayer player1;
        public GamePlayer player2;

        public void EnterGameRoom(ClientInfo user1, ClientInfo user2,int RoomNumber)
        {
            this.gameRoomNumber = RoomNumber;

            player1 = new GamePlayer(user1, 1, RoomNumber);
            player2 = new GamePlayer(user2, 2, RoomNumber);

            user1.EnterClientGameRoom(player1, this);
            user2.EnterClientGameRoom(player2, this);
        }

        public void GameStart()
        {
            Random random = new Random();
            
            HandleGamePacket player1GamePacket = new HandleGamePacket();
            player1GamePacket.startGame = true;
            //player1GamePacket.card = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            player1GamePacket.playerTurn = 1;
            SendGameStartMessage(Header.Game, player1GamePacket, player1.owner);

            HandleGamePacket player2GamePacket = new HandleGamePacket();
            player2GamePacket.startGame = true;
            //player2GamePacket.card = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            player1GamePacket.playerTurn = 2;
            SendGameStartMessage(Header.Game, player2GamePacket, player2.owner);


            //if(player1GamePacket.card < player2GamePacket.card)
            //{
            //    player1GamePacket.playerTurn = 1;
            //    player2GamePacket.playerTurn = 2;
            //}

            //else if (player1GamePacket.card > player2GamePacket.card)
            //{
            //    player1GamePacket.playerTurn = 2;
            //    player2GamePacket.playerTurn = 1;
            //}

            ////두 카드가 같을 경우 다시 셔플
            //else 
            //{
            //    player1GamePacket.card = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            //    player2GamePacket.card = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            //}

            //SendGameStartMessage(Header.Game, player1GamePacket, player1.owner);
            //SendGameStartMessage(Header.Game, player2GamePacket, player2.owner);
        }

        public void RequestBetting(GamePlayer player,int bettingParam)
        {
            IndianPokerGamePacket pokerGamePacket = new IndianPokerGamePacket();
            
            pokerGamePacket.betting = bettingParam;
            
            if(player.PlayerIndex == 1)
            {
                SendPokerGameMessage(Header.GameMotion, pokerGamePacket, player2.owner);
            }
            else
            {
                SendPokerGameMessage(Header.GameMotion, pokerGamePacket, player1.owner);
            }
            //currentTurnPlayer = player.PlayerIndex;

            //if (currentTurnPlayer == player1.PlayerIndex)
            //{

            //}
            //else if (currentTurnPlayer == player2.PlayerIndex)
            //{

            //}

            
        }
    }

    public class GameRoomManager
    {
        int gameRoomIndex = 0;
        public Dictionary<int, GameRoom> GameRoomDic = new Dictionary<int, GameRoom>();
        //public List<GameRoom> gameRoomList = new List<GameRoom>();

        //public GameRoomManager(ClientInfo Player1, ClientInfo Player2)
        //{
        //    gameRoom = new GameRoom(Player1, Player2);
        //    gameRoomList.Add(gameRoom);
        //}

        public GameRoomManager()
        {
            
        }

        public int CreateGameRoom(ClientInfo Player1, ClientInfo Player2)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.EnterGameRoom(Player1, Player2, gameRoomIndex);
            GameRoomDic.Add(gameRoomIndex, gameRoom);
            gameRoomIndex++;
            return gameRoomIndex - 1;
        }

        public void DestroyGameRoom(int roomIndex, GameRoom gameRoom)
        {
            GameRoomDic.Remove(roomIndex);
        }
    }
}
