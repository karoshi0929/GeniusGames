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

namespace GameUser
{
    /// <summary>
    /// UCIndianPoker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCIndianPoker : UserControl
    {
        public delegate void DelegateSendGameBettingMessage(DataHandler.IndianPokerGamePacket gamePacket);
        public DelegateSendGameBettingMessage SendGamePacketMessage;
        public UCIndianPoker()
        {
            InitializeComponent();
        }

        private void Button_Die_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.IndianPokerGamePacket gamePacket = new DataHandler.IndianPokerGamePacket();

        }

        private void Button_Bbing_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.IndianPokerGamePacket gamePacket = new DataHandler.IndianPokerGamePacket();
        }

        private void Button_Double_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.IndianPokerGamePacket gamePacket = new DataHandler.IndianPokerGamePacket();
        }

        private void Button_Check_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.IndianPokerGamePacket gamePacket = new DataHandler.IndianPokerGamePacket();
        }

        private void Button_Queter_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.IndianPokerGamePacket gamePacket = new DataHandler.IndianPokerGamePacket();
        }

        private void Button_Half_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.IndianPokerGamePacket gamePacket = new DataHandler.IndianPokerGamePacket();
        }

        private void Button_Ready_Click(object sender, RoutedEventArgs e)
        {
            DataHandler.IndianPokerGamePacket gamePacket = new DataHandler.IndianPokerGamePacket();
            gamePacket.loadingComplete = true;
            gamePacket.startGame = false;
            gamePacket.betting = 0;
            gamePacket.card = 0;
            gamePacket.playerTurn = 0;

            SendGamePacketMessage(gamePacket);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Button_Ready.Visibility = Visibility.Collapsed;
            }));
        }
    }
}
