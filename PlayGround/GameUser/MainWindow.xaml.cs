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

        IndianPokerClient indianPokserClient;
        PacketDefine packetDefine = new PacketDefine();
        

        public MainWindow()
        {
            InitializeComponent();
            this.LoginScreen.Loginbtn_event += LoginScreen_Loginbtn_event;
            this.SelectGameScreen.indianbtn_event += SelectedGameScreen_SelectGame;
            this.SelectGameScreen.mazebtn_event += SelectedGameScreen_SelectGame;
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
                    this.LoginScreen.Visibility = Visibility.Collapsed;
                    this.SelectGameScreen.Visibility = Visibility.Collapsed;
                    this.IndianPokerScreen.Visibility = Visibility.Visible;
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
            
            short id = short.Parse(this.LoginScreen.IDboxString);
            this.indianPokserClient = new IndianPokerClient("192.168.2.60", 10000, id);
            
            if (this.indianPokserClient.ConnectedServer())
            {
                this.SetVisible(Screen.SelectedGame);

                LoginData loginData = new LoginData();
                loginData.clientID = id.ToString();
                loginData.isLogin = true;
                loginData.Ack = 1;

                indianPokserClient.SendMessage(Header.Login, loginData);
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
                    this.SetVisible(Screen.IndianPoker);
                    if(indianPokserClient.RequestMatch()) // 서버에 매치요청 보냄
                    {

                    }
                    break;
                case "Set Maze of Memory Screen":
                    this.SetVisible(Screen.MazeofMemory);
                    break;
                default:
                    break;
            }
        }
    }
}
