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
        //private int otherPlayerMoney = 0;
        private int totalBettingMoney = 0;
        

        //첫번째 턴일때는 쿼터,하프는 총베팅액기준으로 계산
        private bool isFirstTurn = false;

        public UCIndianPoker()
        {
            InitializeComponent();
        }

        private void Button_Call_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.Betting = (int)Betting.BettingCall;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Die_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.Betting = (int)Betting.BettingDie;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Double_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.Betting = (int)Betting.BettingDouble;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Check_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.Betting = (int)Betting.BettingCheck;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Queter_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            gamePacket.Betting = (int)Betting.BettingQueter;

            SendGamePacketMessage(gamePacket);
        }

        private void Button_Half_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            SendBetting(Betting.BettingHalf);
            //IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();
            //gamePacket.Betting = (int)Betting.BettingHalf;

            //gamePacket.MyMoney = myMoney;
            //gamePacket.BettingMoney = bettingMoney;

            //SendGamePacketMessage(gamePacket);
        }

        public void SendBetting(Betting betting)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();

            int bettingMoney = 0;

            switch (betting)
            {
                case Betting.BettingCall:
                    break;
                case Betting.BettingDie:
                    break;
                case Betting.BettingDouble:
                    break;
                case Betting.BettingCheck:
                    if (isFirstTurn)
                    {
                        //첫번째 베팅일땐 체크버튼 활성화
                    }
                    else
                    {
                        //첫번째 베팅일땐 체크버튼 비활성화
                    }
                    break;
                case Betting.BettingQueter:
                    if (isFirstTurn)
                    {
                        //첫번째 베팅일땐 총금액의 1/4
                    }
                    else
                    {

                    }
                    break;
                case Betting.BettingHalf:
                    if (isFirstTurn)
                    {
                        isFirstTurn = false;

                        bettingMoney = totalBettingMoney / 2;
                        myMoney = myMoney - bettingMoney;
                        totalBettingMoney = totalBettingMoney + bettingMoney;
                    }
                    else
                    {
                        //총베팅금액 + 상대방의 베팅금액 / 2
                        bettingMoney = (totalBettingMoney + otherPlayerBettingMoney) + ((totalBettingMoney + otherPlayerBettingMoney) / 2);
                        myMoney = myMoney - bettingMoney;
                        totalBettingMoney = totalBettingMoney + bettingMoney;

                    }
                    break;
            }

            gamePacket.Betting = (short)betting;
            gamePacket.BettingMoney = bettingMoney;
            gamePacket.MyMoney = myMoney;
            SendGamePacketMessage(gamePacket);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Label_MyMoney.Content = myMoney.ToString();
                Label_BetTotalMoney.Content = totalBettingMoney.ToString();
            }));
        }

        public void ReceiveBetting(Betting betting, IndianPokerGamePacket gamePacketParam)
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
                    otherPlayerBettingMoney = gamePacketParam.BettingMoney;
                    totalBettingMoney = totalBettingMoney + gamePacketParam.BettingMoney;
                    break;
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Label_OtherPlayerMoney.Content = gamePacketParam.OtherPlayerMoney.ToString();
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
                    isFirstTurn = true;
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
