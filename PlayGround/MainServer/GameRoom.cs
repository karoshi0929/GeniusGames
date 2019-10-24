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

        const short CARDMINNUM = 0;
        const short CARDMAXNUM = 20;

        private int currentTurnPlayer;
        private int totalBettingMoney = 0;

        private short[] card = new short[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                                            1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
        short player1Card = 0;
        short player2Card = 0;

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
            HandleGamePacket player2GamePacket = new HandleGamePacket();

            totalBettingMoney = 10;

            player1GamePacket.startGame = true;
            player1GamePacket.MyCard = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            player1Card = player1GamePacket.MyCard;
            player1GamePacket.playerTurn = 1;
            player1GamePacket.TotalBettingMoney = totalBettingMoney;
            player1GamePacket.MyMoney = 995;
            

            player2GamePacket.startGame = true;
            player2GamePacket.MyCard = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            player2Card = player2GamePacket.MyCard;
            player2GamePacket.playerTurn = 2;
            player2GamePacket.TotalBettingMoney = totalBettingMoney;
            player2GamePacket.MyMoney = 995;

            player1GamePacket.OtherPlayerCard = player2GamePacket.MyCard;
            player2GamePacket.OtherPlayerCard = player1GamePacket.MyCard;
            player1GamePacket.OtherPlayerMoney = player2GamePacket.MyMoney;
            player2GamePacket.OtherPlayerMoney = player1GamePacket.MyMoney;

            SendGameStartMessage(Header.Game, player1GamePacket, player1.owner);
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

        public void RequestBetting(GamePlayer player, IndianPokerGamePacket gamePacketParam)
        {
            IndianPokerGamePacket pokerGamePacket = new IndianPokerGamePacket();
            
            pokerGamePacket.Betting = gamePacketParam.Betting;
            pokerGamePacket.BettingMoney = gamePacketParam.BettingMoney;
            pokerGamePacket.OtherPlayerMoney = gamePacketParam.MyMoney;
            if (player.PlayerIndex == 1)
            {
                //totalBettingMoney += pokerGamePacket.betting;
                SendPokerGameMessage(Header.GameMotion, pokerGamePacket, player2.owner);
            }
            else
            {
                SendPokerGameMessage(Header.GameMotion, pokerGamePacket, player1.owner);
            }
            

            if(gamePacketParam.Betting == (short)Betting.BettingCall)
            {
                if(player1Card > player2Card)
                {
                    //두 플레이어 에게 결과 송신
                }
                else
                {
                    //두 플레이어 에게 결과 송신
                }
            }
            else if(gamePacketParam.Betting == (short)Betting.BettingDie)
            {

            }
            else
            {

            }
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
