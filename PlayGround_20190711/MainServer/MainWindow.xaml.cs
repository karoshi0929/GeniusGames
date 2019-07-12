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

        DataHandler.EventManager eventManager = new DataHandler.EventManager();

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
            indianPokerServer.printText += PrintText;

            eventManager.RequestMatchingEvent += EventManager_RequestMatchingEvent;
        }

        private void EventManager_RequestMatchingEvent(StartMatching message)
        {
            //클라이언트로부터 매칭 시작 메세지 받았을 시
            if(message.matchingMsg == (byte)Matching.StartMatching)
            {

            }
            //클라이언트로부터 매칭 멈춤 메세지 받았을 시
            else if (message.matchingMsg == (byte)Matching.StopMatching)
            {

            }
            //throw new NotImplementedException();
        }

        private void PrintText(string message)
        {
            //TextBoxDisplayLog.AppendText = message;
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
