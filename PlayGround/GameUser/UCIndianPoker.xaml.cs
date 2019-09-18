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

namespace GameUser
{
    /// <summary>
    /// UCIndianPoker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCIndianPoker : UserControl
    {
        public delegate void DelegateSendGameBettingMessage(IndianPokerGamePacket gamePacket);
        public DelegateSendGameBettingMessage SendGamePacketMessage;

        int card = 0;
        int money = 0;

        public UCIndianPoker()
        {
            InitializeComponent();
        }

        private void Button_Call_Click(object sender, RoutedEventArgs e)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingCall;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Die_Click(object sender, RoutedEventArgs e)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingDie;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Bbing_Click(object sender, RoutedEventArgs e)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingBbing;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Double_Click(object sender, RoutedEventArgs e)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingDouble;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Check_Click(object sender, RoutedEventArgs e)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingCheck;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Queter_Click(object sender, RoutedEventArgs e)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingQueter;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Half_Click(object sender, RoutedEventArgs e)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingHalf;

            SendGamePacketMessage(gamePacket);
        }

        
    }
}
