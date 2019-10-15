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

        public bool isGameStart = false;

        private int myCard = 0;
        private int myMoney = 0;

        private int otherPlayerBettingMoney = 0;
        private int otherPlayerMoney = 0;
        private int totalBettingMoney = 0;

        public string SendBetting = "Send";
        public string ReceiveBetting = "Receive";


        public UCIndianPoker()
        {
            InitializeComponent();
        }

        private void Button_Call_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingCall;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Die_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingDie;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Double_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingDouble;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Check_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingCheck;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Queter_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingQueter;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Half_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();

            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.betting = (int)Betting.BettingHalf;
            HandleBettingMoney(Betting.BettingHalf, SendBetting);

            SendGamePacketMessage(gamePacket);
        }

        public void HandleBettingMoney(Betting betting, string strAction)
        {
            if(strAction == SendBetting)
            {
                switch (betting)
                {
                    case Betting.BettingCall:
                        break;
                    case Betting.BettingDie:
                        break;
                    case Betting.BettingDouble:
                        break;
                    case Betting.BettingCheck:
                        break;
                    case Betting.BettingQueter:
                        break;
                    case Betting.BettingHalf:
                        //(총베팅금액 + 상대방의 베팅금액) + (총베팅금액 + 상대방의 베팅금액) / 2
                        totalBettingMoney = (totalBettingMoney + otherPlayerBettingMoney) / 2;
                        break;
                }
            }
            else //ReceiveBetting 상대방에게 베팅받았을 때 처리.
            {
                switch (betting)
                {
                    case Betting.BettingCall:
                        break;
                    case Betting.BettingDie:
                        break;
                    case Betting.BettingDouble:
                        break;
                    case Betting.BettingCheck:
                        break;
                    case Betting.BettingQueter:
                        break;
                    case Betting.BettingHalf:
                        totalBettingMoney = totalBettingMoney + 1;
                        break;
                }
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Label_MyMoney.Content = myMoney.ToString();
                Label_BetTotalMoney.Content = totalBettingMoney.ToString();
            }));
        }

        public void SetGameStart(HandleGamePacket gamePacketParam)
        {
            myMoney = gamePacketParam.MyMoney;
            totalBettingMoney = gamePacketParam.TotalBettingMoney;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Button_MyCard.Content = gamePacketParam.MyCard.ToString();
                Button_OtherPlayerCard.Content = gamePacketParam.OtherPlayerCard.ToString();

                Label_MyMoney.Content = gamePacketParam.MyMoney.ToString();
                Label_OtherPlayerMoney.Content = gamePacketParam.OtherPlayerMoney.ToString();

                Label_BetTotalMoney.Content = gamePacketParam.TotalBettingMoney.ToString();
                isGameStart = true;
                if (gamePacketParam.playerTurn == 1)
                {
                    TextBox_UserLog.AppendText("게임이 시작되었습니다. 선턴입니다. 베팅 하세여 \n");
                    Button_Call.IsEnabled = false;
                }
                else
                {
                    TextBox_UserLog.AppendText("게임이 시작되었습니다. 후턴입니다. \n");
                    SetButtonsDisable();
                }
            }));
        }
            

        public void SetButtonsEnable()
        {
            Button_Call.IsEnabled = true;
            Button_Die.IsEnabled = true;
            Button_Double.IsEnabled = true;
            Button_Check.IsEnabled = true;
            Button_Queter.IsEnabled = true;
            Button_Half.IsEnabled = true;
        }
        public void SetButtonsDisable()
        {
            Button_Call.IsEnabled = false;
            Button_Die.IsEnabled = false;
            Button_Double.IsEnabled = false;
            Button_Check.IsEnabled = false;
            Button_Queter.IsEnabled = false;
            Button_Half.IsEnabled = false;
        }
    }
}
