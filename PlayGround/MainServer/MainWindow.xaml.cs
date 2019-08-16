using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
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

        //Queue<ClientInfo> MatchingClientQueue = new Queue<ClientInfo>();
        List<ClientInfo> WaitingMatchClientList = new List<ClientInfo>();

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
            //클라이언트로부터 IndianPoker게임관련 Message를 받았을 때
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
            ClientInfo asd = clientManagement.ClientInfoDic[e.Data.clientID];

            MatchingPacket matchingPacket = e.Data;

            //1. 어떤 게임의 매칭 요청인지 확인
            switch(e.Data.GameID)
            {
                case (byte)KindOfGame.IndianPokser:
                    //클라이언트로부터 매칭 시작 메세지 받았을 시
                    if (e.Data.matchingMsg == (byte)Matching.StartMatching)
                    {
                        //2. 매칭 대기 리스트에 담기
                        WaitingMatchClientList.Add(asd);
                    }
                    //클라이언트로부터 매칭 멈춤 메세지 받았을 시
                    else if (e.Data.matchingMsg == (byte)Matching.StopMatching)
                    {
                        //매칭 리스트에서 제거
                        WaitingMatchClientList.Remove(asd);
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
                matchingPacket.matchingComplete = true;

                //여기서 오류남 수정 해야함.
                indianPokerServer.SendMessage(Header.Matching, matchingPacket, WaitingMatchClientList[0].ClientSocket);
                indianPokerServer.SendMessage(Header.Matching, matchingPacket, WaitingMatchClientList[1].ClientSocket);

                //2. RoomManager에게 클라이언트 전송.
                gameRoomManager.CreateGameRoom(WaitingMatchClientList[0], WaitingMatchClientList[1]);
                
                //3. 매칭리스트에서 클라이언트 제거
                WaitingMatchClientList.Remove(WaitingMatchClientList[0]);
                WaitingMatchClientList.Remove(WaitingMatchClientList[1]);
            }
        }

        private void Instance_IndianPokerGamePacketEvent(DataHandler.EventManager.IndianPokerGamePacketReceivedArgs e)
        {
            ClientInfo asd = clientManagement.ClientInfoDic[e.Data.clientID];

            asd.gameRoom.RequestBetting();
        }

        private void HandleMatching()
        {

        }

        private void PrintText(string message)
        {
            LogMessage = message;
        }

        //프로그램 실행 시 인디언 포커, 기억의 미로 서버 실행
        private void ButtonStartServer_Click(object sender, RoutedEventArgs e)
        {
            indianPokerServer.OpenIndianPokerServer();
        }


    }
}
