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
        //

        public MainWindow()
        {
            InitializeComponent();
            
            this.DataContext = this;

            indianPokerServer = new IndianPokerServer();
            indianPokerServer.printText = new IndianPokerServer.PrintTextDelegate(PrintText);

            clientManagement = new ClientManagement();

            //클라이언트로부터 Login Message를 받았을 때
            DataHandler.EventManager.Instance.LoginPacketEvent += Instance_LoginPacketEvent;
            //클라이언트로부터 GameMatching요청 Message를 받았을 때
            DataHandler.EventManager.Instance.MatchingPacketEvent += Instance_MatchingPacketEvent;
            //클라이언트로부터 IndianPoker게임관련 Message를 받았을 때
            DataHandler.EventManager.Instance.IndianPokerGamePacketEvent += Instance_IndianPokerGamePacketEvent;
        }

        private void Instance_LoginPacketEvent(DataHandler.EventManager.LoginPacketReceivedArgs e)
        {
            //ClientManagement 클래스에 로그인한 클라이언트 정보 저장 
            Console.WriteLine(e.Data.clientID.ToString());
        }

        private void Instance_MatchingPacketEvent(DataHandler.EventManager.MatchingPacketReceivedArgs e)
        {
            //1. 클라이언트 ID 확인

            //클라이언트로부터 매칭 시작 메세지 받았을 시
            if (e.Data.matchingMsg == (byte)Matching.StartMatching)
            {
                //큐에 클라이언트를 담고
                //count == 2가 되면 두 클라이언트 매칭.
                //매칭된 클라이언트에게 MessageSend
            }
            //클라이언트로부터 매칭 멈춤 메세지 받았을 시
            else if (e.Data.matchingMsg == (byte)Matching.StopMatching)
            {

            }
        }

        private void Instance_IndianPokerGamePacketEvent(DataHandler.EventManager.IndianPokerGamePacketReceivedArgs e)
        {

        }

        private void PrintText(string message)
        {
            LogMessage = message;
        }

        private void AddClient()
        {
            //TextBoxDisplayLog.AppendText = message;
            //LogMessage = message;
        }

        //프로그램 실행 시 인디언 포커, 기억의 미로 서버 실행
        private void ButtonStartServer_Click(object sender, RoutedEventArgs e)
        {
            indianPokerServer.OpenIndianPokerServer();
        }


    }
}
