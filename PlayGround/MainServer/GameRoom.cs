﻿using System;
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
        public delegate void DelegateSendGameStartMessage(Header header, IndianPokerGamePacket gamePacket, ClientInfo clientSocket);
        public DelegateSendGameStartMessage SendGameStartMessage;

        const short CARDMINNUM = 1;
        const short CARDMAXNUM = 20;

        int gameRoomNumber = 0;

        GamePlayer player1;
        GamePlayer player2;

        int currentTurnPlayer;
        int totalBettingMoney = 0;

        short[] card = new short[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10};


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
            
            IndianPokerGamePacket player1GamePacket = new IndianPokerGamePacket();
            //player1GamePacket.startGame = 0x01;
            //player1GamePacket.playerTurn = player1.PlyaerIndex;
            player1GamePacket.card = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            SendGameStartMessage(Header.Game, player1GamePacket, player1.owner);

            IndianPokerGamePacket player2GamePacket = new IndianPokerGamePacket();
            //player2GamePacket.startGame = 0x01;
            //gamePacket.playerTurn = player1.PlyaerIndex;
            player2GamePacket.card = (short)random.Next(CARDMINNUM, CARDMAXNUM);
            SendGameStartMessage(Header.Game, player2GamePacket, player2.owner);
        }

        public void RequestBetting()
        {
            if(currentTurnPlayer == player1.PlyaerIndex)
            {

            }
            //if(currentTurnPlayer == player2.PlyaerIndex)
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
