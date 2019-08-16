﻿using System;
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

        private IndianPokerClient indianPokserClient;
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
            DataHandler.EventManager.Instance.MatchingPacketEvent += Instance_MatchingPacketEvent;
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
            this.indianPokserClient = new IndianPokerClient("127.0.0.1", 10000, this.ClientID);

            if (this.indianPokserClient.ConnectedServer())
            {
                this.SetVisible(Screen.SelectedGame);

                LoginPacket loginPacket = new LoginPacket();
                loginPacket.clientID = this.ClientID;
                loginPacket.isLogin = true;
                loginPacket.Ack = 1;

                indianPokserClient.SendMessage(Header.Login, loginPacket);
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
                matchingPacket.Ack = 1;

                indianPokserClient.SendMessage(Header.Matching, matchingPacket);

                //기다리는 화면 표시
            }
        }

        //서버에서 매칭 메시지 수신 시 게임 화면 표시
        private void Instance_MatchingPacketEvent(DataHandler.EventManager.MatchingPacketReceivedArgs e)
        {
            if(e.Data.matchingComplete)
            {
                this.SetVisible(Screen.IndianPoker);
            }

            //this.isMatching = true;
            //if (!this.isPlaying)
            //{
            //    this.SetVisible(Screen.IndianPoker);
            //    this.isPlaying = true;
            //}
        }


    }
}
