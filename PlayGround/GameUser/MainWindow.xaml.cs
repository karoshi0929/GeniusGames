using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using TCPcommunication;

namespace GameUser
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        private IndianPokerClient indianPokerClient;
        private PacketDefine packetDefine = new PacketDefine();
        //private DataHandler.EventManager manager = new DataHandler.EventManager();

        private bool isMatching = false;
        private bool isPlaying = false;

        private string ClientID;
        public MainWindow()
        {
            InitializeComponent();
            this.LoginScreen.Loginbtn_event += LoginScreen_Loginbtn_event;
            this.SelectGameScreen.indianbtn_event += SelectedGameScreen_SelectGame;
            this.SelectGameScreen.mazebtn_event += SelectedGameScreen_SelectGame;

            IndianPokerScreen.SendGamePacketMessage += new UCIndianPoker.DelegateSendGameBettingMessage(SendGameMessage);
            IndianPokerScreen.SendNewGameMessage += new UCIndianPoker.DelegateSendNewGameStartMessage(StartNewGameMessage);

            DataHandler.EventManager.Instance.MatchingPacketEvent += Instance_MatchingPacketEvent;
            DataHandler.EventManager.Instance.HandleGamePacketEvent += Instance_HandleGamePacketEvent;
            DataHandler.EventManager.Instance.IndianPokerGamePacketEvent += Instance_IndianPokerGamePacketEvent;
        }

        private void SetVisible(Screen selectedscreen)
        {
            switch (selectedscreen)
            {
                case Screen.Login:
                    break;
                case Screen.SelectedGame:
                    this.LoginScreen.Visibility = Visibility.Collapsed;
                    this.SelectGameScreen.Visibility = Visibility.Visible;
                    this.IndianPokerScreen.Visibility = Visibility.Collapsed;
                    break;

                case Screen.IndianPoker:
                    Dispatcher.Invoke(new Action(() =>
                    {
                        this.LoginScreen.Visibility = Visibility.Collapsed;
                        this.SelectGameScreen.Visibility = Visibility.Collapsed;
                        this.IndianPokerScreen.Visibility = Visibility.Visible;
                        this.isPlaying = true;
                    }));
                    break;

                case Screen.MazeofMemory:
                    this.LoginScreen.Visibility = Visibility.Collapsed;
                    this.SelectGameScreen.Visibility = Visibility.Collapsed;
                    //this.MazeofMemoryScreen.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
        enum Screen
        {
            Login,
            SelectedGame,
            IndianPoker,
            MazeofMemory,
        }

        private void LoginScreen_Loginbtn_event(string message)
        {
            this.ClientID = this.LoginScreen.IDboxString;
            this.indianPokerClient = new IndianPokerClient("127.0.0.1", 10000, this.ClientID);

            if (this.indianPokerClient.ConnectedServer())
            {
                this.SetVisible(Screen.SelectedGame);

                LoginPacket loginPacket = new LoginPacket();
                loginPacket.clientID = this.ClientID;
                loginPacket.isLogin = true;
                loginPacket.Ack = 0x01;

                indianPokerClient.SendMessage(Header.Login, loginPacket, indianPokerClient.ao.WorkingSocket);
            }
            else
            {
                MessageBox.Show("서버와 연결이 되지 않습니다.");
            }
        }

        private void SelectedGameScreen_SelectGame(string message)
        {
            switch (message)
            {
                case "Set Indian Poker Screen":
                    SetScreen();
                    break;
                case "Set Maze of Memory Screen":
                    this.SetVisible(Screen.MazeofMemory);
                    break;
                default:
                    break;
            }
        }
        //서버에 매칭 요청 메시지 송신
        private void SetScreen()
        {
            if (!this.isMatching)
            {
                MatchingPacket matchingPacket = new MatchingPacket();
                matchingPacket.clientID = this.ClientID;
                matchingPacket.GameID = (byte)KindOfGame.IndianPokser;
                matchingPacket.matchingMsg = (byte)Matching.StartMatching;
                matchingPacket.Ack = 0x01;

                indianPokerClient.SendMessage(Header.Matching, matchingPacket, indianPokerClient.ao.WorkingSocket);

                //기다리는 화면 표시
            }
        }

        //서버에서 매칭 메시지 수신 시 게임 화면 표시
        private void Instance_MatchingPacketEvent(DataHandler.EventManager.MatchingPacketReceivedArgs e)
        {
            if(e.Data.matchingComplete)
            {
                this.SetVisible(Screen.IndianPoker);

                if(isPlaying)
                {
                    HandleGamePacket handleGamePakcet = new HandleGamePacket();
                    handleGamePakcet.clientID = this.ClientID;
                    handleGamePakcet.loadingComplete = true;
                    handleGamePakcet.startGame = false;
                    handleGamePakcet.MyCard = 0;
                    handleGamePakcet.playerTurn = 0;

                    indianPokerClient.SendMessage(Header.Game, handleGamePakcet, indianPokerClient.ao.WorkingSocket);
                }
            }
        }

        private void Instance_HandleGamePacketEvent(DataHandler.EventManager.HandleGamePacketReceivedArgs e)
        {
            IndianPokerScreen.SetGameStart(e.Data);
        }

        private void Instance_IndianPokerGamePacketEvent(DataHandler.EventManager.IndianPokerGamePacketReceivedArgs e)
        {
            IndianPokerScreen.ReceiveBetting((Betting)e.Data.Betting, e.Data);
        }

        private void SendGameMessage(IndianPokerGamePacket gamePacketParam)
        {
            gamePacketParam.clientID = this.ClientID;
            indianPokerClient.SendMessage(Header.GameMotion, gamePacketParam, indianPokerClient.ao.WorkingSocket);
        }
        
        private void StartNewGameMessage(HandleGamePacket handleGamePacketParam)
        {
            handleGamePacketParam.clientID = this.ClientID;
            indianPokerClient.SendMessage(Header.Game, handleGamePacketParam, indianPokerClient.ao.WorkingSocket);
        }
    }
}