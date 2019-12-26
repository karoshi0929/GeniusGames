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
        const short CARDMAXNUM = 10;

        private short victoryUser = 0;
        private short currentTurnPlayer;
        private int totalBettingMoney = 0;

        private int[] card = new int[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                                        1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        private short player1Card = 0;
        private short player2Card = 0;

        public int gameRoomNumber = 0;
        public bool playGame = false;

        public GamePlayer player1;
        public GamePlayer player2;

        public void EnterGameRoom(ClientInfo user1, ClientInfo user2,int RoomNumber)
        {
            this.gameRoomNumber = RoomNumber;

            player1 = new GamePlayer(user1, 1, RoomNumber, 1);
            player2 = new GamePlayer(user2, 2, RoomNumber, 2);

            user1.EnterClientGameRoom(player1, this);
            user2.EnterClientGameRoom(player2, this);

            ShuffleRandom();
        }

        public void GameStart()
        {
            player1.isReadyForGame = false;
            player2.isReadyForGame = false;

            Random random = new Random();
            
            HandleGamePacket player1GamePacket = new HandleGamePacket();
            HandleGamePacket player2GamePacket = new HandleGamePacket();

            totalBettingMoney = 10;

            player1GamePacket.startGame = true;
            player1GamePacket.MyIndex = player1.PlayerIndex;
            player1GamePacket.MyCard = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            player1Card = player1GamePacket.MyCard;

            if(victoryUser == 0)
                player1.PlayerTurn = 1;
            else if(victoryUser == 1)
                player1.PlayerTurn = 1;
            else
                player1.PlayerTurn = 2;

            player1GamePacket.playerTurn = player1.PlayerTurn;
            player1GamePacket.TotalBettingMoney = totalBettingMoney;
            player1GamePacket.MyMoney = player1.PlayerMoney - 5;

            player2GamePacket.startGame = true;
            player2GamePacket.MyIndex = player2.PlayerIndex;
            player2GamePacket.MyCard = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            player2Card = player2GamePacket.MyCard;

            if (victoryUser == 0)
                player2.PlayerTurn = 2;
            else if (victoryUser == 2)
                player2.PlayerTurn = 1;
            else
                player2.PlayerTurn = 2;
            player2GamePacket.playerTurn = player2.PlayerTurn;

            player2GamePacket.TotalBettingMoney = totalBettingMoney;
            player2GamePacket.MyMoney = player2.PlayerMoney - 5;

            player1GamePacket.OtherPlayerCard = player2GamePacket.MyCard;
            player2GamePacket.OtherPlayerCard = player1GamePacket.MyCard;
            player1GamePacket.OtherPlayerMoney = player2GamePacket.MyMoney;
            player2GamePacket.OtherPlayerMoney = player1GamePacket.MyMoney;

            SendGameStartMessage(Header.Game, player1GamePacket, player1.owner);
            SendGameStartMessage(Header.Game, player2GamePacket, player2.owner);
        }

        public void RequestBetting(GamePlayer player, IndianPokerGamePacket gamePacketParam)
        {
            /////////////////////////////////////////////////////////////////////
            if(player.PlayerIndex == 1)
            {
                player1.PlayerMoney = gamePacketParam.MyMoney;
            }
            else
            {
                player2.PlayerMoney = gamePacketParam.MyMoney;
            }
            /////////////////////////////////////////////////////////////////////

            //IndianPokerGamePacket pokerGamePacket = gamePacketParam;

            this.totalBettingMoney = this.totalBettingMoney + gamePacketParam.BettingMoney;

            if (gamePacketParam.Betting == (short)Betting.BettingCall)
            {
                IndianPokerGamePacket SendToPlayer1 = new IndianPokerGamePacket();
                IndianPokerGamePacket SendToPlayer2 = new IndianPokerGamePacket();

                if (player.PlayerIndex == 1)
                {
                    if(player1Card > player2Card)
                    {
                        player1.PlayerMoney = player1.PlayerMoney + this.totalBettingMoney;
                        victoryUser = 1;

                        SendToPlayer2 = gamePacketParam;
                        SendToPlayer2.VictoryUser = victoryUser;
                        SendToPlayer2.playerTurn = player2.PlayerTurn;

                        SendToPlayer1.MyMoney = player2.PlayerMoney;
                        SendToPlayer1.VictoryUser = victoryUser;
                    }
                    else if(player1Card < player2Card)
                    {
                        player2.PlayerMoney = player2.PlayerMoney + this.totalBettingMoney;
                        victoryUser = 2;

                        SendToPlayer2 = gamePacketParam;
                        SendToPlayer2.VictoryUser = victoryUser;
                        SendToPlayer2.playerTurn = player2.PlayerTurn;

                        SendToPlayer1.MyMoney = player2.PlayerMoney;
                        SendToPlayer1.VictoryUser = victoryUser;
                    }
                }

                else
                {
                    if (player1Card > player2Card)
                    {
                        player1.PlayerMoney = player1.PlayerMoney + this.totalBettingMoney;
                        victoryUser = 1;

                        SendToPlayer1 = gamePacketParam;
                        SendToPlayer1.VictoryUser = victoryUser;
                        SendToPlayer1.playerTurn = player1.PlayerTurn;

                        SendToPlayer2.MyMoney = player1.PlayerMoney;
                        SendToPlayer2.VictoryUser = victoryUser;
                    }
                    else if (player1Card < player2Card)
                    {
                        player2.PlayerMoney = player2.PlayerMoney + this.totalBettingMoney;
                        victoryUser = 2;

                        SendToPlayer1 = gamePacketParam;
                        SendToPlayer1.VictoryUser = victoryUser;
                        SendToPlayer1.playerTurn = player1.PlayerTurn;

                        SendToPlayer2.MyMoney = player2.PlayerMoney;
                        SendToPlayer2.VictoryUser = victoryUser;
                    }
                }

                SendPokerGameMessage(Header.GameMotion, SendToPlayer1, player1.owner);
                SendPokerGameMessage(Header.GameMotion, SendToPlayer2, player2.owner);
            }

            else if(gamePacketParam.Betting == (short)Betting.BettingDie)
            {
                IndianPokerGamePacket SendToPlayer1 = new IndianPokerGamePacket();
                IndianPokerGamePacket SendToPlayer2 = new IndianPokerGamePacket();

                if (player.PlayerIndex == 1)
                {
                    SendToPlayer2 = gamePacketParam;
                    player2.PlayerMoney = player2.PlayerMoney + this.totalBettingMoney;
                    victoryUser = 2;
                    SendToPlayer2.VictoryUser = victoryUser;
                    SendToPlayer2.playerTurn = player2.PlayerTurn;

                    SendToPlayer1.MyMoney = player2.PlayerMoney; //이부분 수정 필요
                    SendToPlayer1.VictoryUser = victoryUser;
                    SendToPlayer1.Betting = gamePacketParam.Betting;
                    SendToPlayer1.BettingMoney = 0;
                }
                else
                {
                    SendToPlayer1 = gamePacketParam;
                    player1.PlayerMoney = player1.PlayerMoney + this.totalBettingMoney;
                    victoryUser = 1;
                    SendToPlayer1.VictoryUser = victoryUser;
                    SendToPlayer1.playerTurn = player1.PlayerTurn;

                    SendToPlayer2.MyMoney = player1.PlayerMoney; //이부분 수정 필요
                    SendToPlayer2.VictoryUser = victoryUser;
                    SendToPlayer2.Betting = gamePacketParam.Betting;
                    SendToPlayer2.BettingMoney = 0;
                }

                SendPokerGameMessage(Header.GameMotion, SendToPlayer1, player1.owner);
                SendPokerGameMessage(Header.GameMotion, SendToPlayer2, player2.owner);
            }

            else
            {
                IndianPokerGamePacket SendToPlayer = gamePacketParam;
                SendToPlayer.playerIndex = player.PlayerIndex;

                if (player.PlayerTurn == 1)
                    SendToPlayer.playerTurn = 2;
                else
                    SendToPlayer.playerTurn = 1;

                if (player.PlayerIndex == 1)
                    SendPokerGameMessage(Header.GameMotion, SendToPlayer, player2.owner);
                else
                    SendPokerGameMessage(Header.GameMotion, SendToPlayer, player1.owner);
            }

        }

        public void EndGame(GamePlayer gamePlayer, HandleGamePacket hanelGamePacketPram)
        {
            if(gamePlayer.PlayerIndex == 1)
            {
                HandleGamePacket player2GamePacket = new HandleGamePacket();
                player2GamePacket.startGame = hanelGamePacketPram.startGame;
                SendGameStartMessage(Header.Game, player2GamePacket, player2.owner);
            }
            else
            {
                HandleGamePacket player1GamePacket = new HandleGamePacket();
                player1GamePacket.startGame = hanelGamePacketPram.startGame;
                SendGameStartMessage(Header.Game, player1GamePacket, player1.owner);
            }

            player1.owner.ExitGameRoom();
            player2.owner.ExitGameRoom();

            GameRoomManager gameroomManager = new GameRoomManager();
            gameroomManager.DestroyGameRoom(gameRoomNumber, this);
        }

        private void ShuffleRandom()
        {

            Random random = new Random();
            int lastIndex = card.Length;

            while (lastIndex > 0)
            {
                lastIndex--;
                int randomIndex = random.Next(lastIndex);
                int temp = card[randomIndex];
                card[randomIndex] = card[lastIndex];
                card[lastIndex] = temp;
            }

        }
    }

    public class GameRoomManager
    {
        int gameRoomIndex = 1;
        public Dictionary<int, GameRoom> GameRoomDic = new Dictionary<int, GameRoom>();

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
