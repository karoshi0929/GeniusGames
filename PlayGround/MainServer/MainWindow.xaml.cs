using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataHandler;

namespace MainServer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        IndianPokerServer indianPokerServer;
        ClientManagement clientManagement;
        GameRoomManager gameRoomManager;

        //List<MatchingPacket> responseMatching = new List<MatchingPacket>();
        List<ClientInfo> WaitingMatchClientList = new List<ClientInfo>();

        MatchingPacket SendUser1MatchingPacket = new MatchingPacket();
        MatchingPacket SendUser2MatchingPacket = new MatchingPacket();

        private int gameRoomNumber = 0;

        /********************************************************************************/
        //TextBox에 출력할 문자열(LogMessage)
        private string strLogMessage = string.Empty;
        public string LogMessage
        {
            get { return strLogMessage; }
            set
            {
                strLogMessage = strLogMessage + Environment.NewLine + value;
                OnPropertyChanged("LogMessage");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /********************************************************************************/

        public MainWindow()
        {
            InitializeComponent();
            
            this.DataContext = this;

            indianPokerServer = new IndianPokerServer();
            indianPokerServer.printText = new IndianPokerServer.PrintTextDelegate(PrintText);

            clientManagement = new ClientManagement();
            gameRoomManager = new GameRoomManager();

            //클라이언트로부터 Login Message를 받았을 때
            DataHandler.EventManager.Instance.LoginPacketEvent += Instance_LoginPacketEvent;
            //클라이언트로부터 GameMatching요청 Message를 받았을 때
            DataHandler.EventManager.Instance.MatchingPacketEvent += Instance_MatchingPacketEvent;
            //클라이언트로부터 게임로딩 및 시작 Message를 받았을 때
            DataHandler.EventManager.Instance.HandleGamePacketEvent += Instance_HandleGamePacketEvent;
            //클라이언트로부터 IndianPoker게임 베팅 Message를 받았을 때
            DataHandler.EventManager.Instance.IndianPokerGamePacketEvent += Instance_IndianPokerGamePacketEvent;
        }

        private void Instance_LoginPacketEvent(DataHandler.EventManager.LoginPacketReceivedArgs e)
        {
            //클라이언트 정보 저장
            ClientInfo clientInfo = new ClientInfo(e.Data, e.ClientSocket);
            if(clientManagement.AddClient(clientInfo))
            {

            }
        }

        private void Instance_MatchingPacketEvent(DataHandler.EventManager.MatchingPacketReceivedArgs e)
        {
            //0. 클라이언트 정보 가지고 오기 Param = 클라이언트 아이디
            ClientInfo clientInfo = clientManagement.ClientInfoDic[e.Data.clientID];

            //매칭요청패킷 저장하기위한 코드, 맘에 안든다 어떻게 고칠지 생각해보자.
            if(SendUser1MatchingPacket.clientID == null)
            {
                SendUser1MatchingPacket = e.Data;
            }
            else
            {
                SendUser2MatchingPacket = e.Data;
            }

            //1. 어떤 게임의 매칭 요청인지 확인
            switch (e.Data.GameID)
            {
                case (byte)KindOfGame.IndianPokser:
                    //클라이언트로부터 매칭 시작 메세지 받았을 시
                    if (e.Data.matchingMsg == (byte)Matching.StartMatching)
                    {
                        //2. 매칭 대기 리스트에 담기
                        WaitingMatchClientList.Add(clientInfo);
                        PrintText("유저 [" + clientInfo.ClientID + "] " + clientInfo.ClientSocket.RemoteEndPoint.ToString() + " 님께서 인디언포커 게임 매칭요청 하였습니다.");
                    }
                    //클라이언트로부터 매칭 멈춤 메세지 받았을 시
                    else if (e.Data.matchingMsg == (byte)Matching.StopMatching)
                    {
                        //매칭 리스트에서 제거
                        WaitingMatchClientList.Remove(clientInfo);
                    }
                    break;
                case (byte)KindOfGame.MazeOfMemory:
                    break;
                case (byte)KindOfGame.RememberNumber:
                    break;
                case (byte)KindOfGame.FinishedAndSum:
                    break;

                default:
                    break;
            }

            //count == 2가 되면 두 클라이언트 매칭.
            //매칭된 클라이언트에게 MessageSend
            //RoomManager에게 클라이언트 정보 전송
            if(WaitingMatchClientList.Count == 2)
            {
                //1. 매칭 성사된 클라이언트에게 SendMessage
                SendUser1MatchingPacket.matchingComplete = true;
                SendUser2MatchingPacket.matchingComplete = true;

                indianPokerServer.SendMessage(Header.Matching, SendUser1MatchingPacket, WaitingMatchClientList[0].ClientSocket);
                indianPokerServer.SendMessage(Header.Matching, SendUser2MatchingPacket, WaitingMatchClientList[1].ClientSocket);

                //2. RoomManager에게 클라이언트 정보 전송.
                gameRoomNumber = gameRoomManager.CreateGameRoom(WaitingMatchClientList[0], WaitingMatchClientList[1]);

                //3. 매칭리스트에서 클라이언트 제거
                WaitingMatchClientList.Clear();

                //4. 매칭패킷 저장 객체 초기화
                SendUser1MatchingPacket = new MatchingPacket();
                SendUser2MatchingPacket = new MatchingPacket();
            }
        }
        private void Instance_HandleGamePacketEvent(DataHandler.EventManager.HandleGamePacketReceivedArgs e)
        {
            //1. 로딩이 완료된 
            ClientInfo clientInfo = clientManagement.ClientInfoDic[e.Data.clientID];

            if (clientInfo.IsPlayGame == false && e.Data.loadingComplete == true)
            {
                clientInfo.IsPlayGame = true;

                //방법 1.
                clientInfo.gamePlayer.isReadyForGame = true;

                if (clientInfo.gameRoom.player1.isReadyForGame && clientInfo.gameRoom.player2.isReadyForGame)
                {
                    clientInfo.gameRoom.SendGameStartMessage += new GameRoom.DelegateSendGameStartMessage(SendGameStartMessage);
                    clientInfo.gameRoom.SendPokerGameMessage += new GameRoom.DelegateSendPokerMessage(SendPokerGameMessage);
                    clientInfo.gameRoom.GameStart();
                }

                //방법 2. 
                #region
                //결과는 같으나 의미하는 코드가 무엇이 더 정확한지 생각해봐야함. 
                //int playerIndex = clientInfo.gamePlayer.PlyaerIndex;
                //switch (playerIndex)
                //{
                //    case 1:
                //      gameRoomManager.GameRoomDic[clientInfo.gameRoom.gameRoomNumber].player1.isReadyForGame = true;
                //      clientInfo.gameRoom.player1.isReadyForGame = true;
                //    break;
                //    case 2:
                //      gameRoomManager.GameRoomDic[clientInfo.gameRoom.gameRoomNumber].player2.isReadyForGame = true;
                //      clientInfo.gameRoom.player2.isReadyForGame = true;
                //    break;
                //}

                //if (gameRoomManager.GameRoomDic[clientInfo.gameRoom.gameRoomNumber].player1.isReadyForGame &&
                //    gameRoomManager.GameRoomDic[clientInfo.gameRoom.gameRoomNumber].player2.isReadyForGame)
                //{
                //    gameRoomManager.GameRoomDic[clientInfo.gameRoom.gameRoomNumber].SendGameStartMessage += new GameRoom.DelegateSendGameStartMessage(SendGameStartMessage);
                //    gameRoomManager.GameRoomDic[clientInfo.gameRoom.gameRoomNumber].GameStart();
                //}
                #endregion

                //방법 1-2 / 2-2.
                #region
                //Random random = new Random();

                //IndianPokerGamePacket player1GamePacket = new IndianPokerGamePacket();
                //player1GamePacket.startGame = true;
                //player1GamePacket.playerTurn = player1.PlyaerIndex;
                //player1GamePacket.card = (short)random.Next(CARDMINNUM, CARDMAXNUM);
                //SendGameStartMessage(Header.Game, player1GamePacket, player1.owner);

                //IndianPokerGamePacket player2GamePacket = new IndianPokerGamePacket();
                //player2GamePacket.startGame = true;
                //player2GamePacket.playerTurn = player1.PlyaerIndex;
                //player2GamePacket.card = (short)random.Next(CARDMINNUM, CARDMAXNUM);
                //SendGameStartMessage(Header.Game, player2GamePacket, player2.owner);
                #endregion
            }

            PrintText(e.Data.clientID);
        }

        private void Instance_IndianPokerGamePacketEvent(DataHandler.EventManager.IndianPokerGamePacketReceivedArgs e)
        {
            ClientInfo clientInfo = clientManagement.ClientInfoDic[e.Data.clientID];
            //int betting = e.Data.Betting;

            clientInfo.gameRoom.RequestBetting(clientInfo.gamePlayer, e.Data);

            PrintText("[" + clientInfo.ClientID + "] " + "로 부터 " + e.Data.Betting.ToString() + "베팅 받았습니다.");
        }

        private void PrintText(string message)
        {
            LogMessage = message;
        }

        public void SendGameStartMessage(Header header, DataHandler.HandleGamePacket gameHandlePacket, ClientInfo clientInfoParam)
        {
            indianPokerServer.SendMessage(header, gameHandlePacket, clientInfoParam.ClientSocket);
        }

        private void SendPokerGameMessage(Header header, DataHandler.IndianPokerGamePacket pokerGamePacket, ClientInfo clientInfoParam)
        {
            indianPokerServer.SendMessage(header, pokerGamePacket, clientInfoParam.ClientSocket);
        }

        //프로그램 실행 시 인디언 포커, 기억의 미로 서버 실행
        private void ButtonStartServer_Click(object sender, RoutedEventArgs e)
        {
            indianPokerServer.OpenIndianPokerServer();
        }
    }
}
