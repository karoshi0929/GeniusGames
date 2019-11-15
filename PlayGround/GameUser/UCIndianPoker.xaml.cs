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
using System.Threading;

namespace GameUser
{
    /// <summary>
    /// UCIndianPoker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCIndianPoker : UserControl
    {
        public delegate void DelegateSendGameBettingMessage(IndianPokerGamePacket gamePacket);
        public DelegateSendGameBettingMessage SendGamePacketMessage;

        public delegate void DelegateSendNewGameStartMessage(HandleGamePacket handleGamePacket);
        public DelegateSendNewGameStartMessage SendNewGameMessage;

        public bool isPlayGame = false;

        private short myCard = 0;
        private short otherPlayerCard = 0;
        private short myIndex = 0;
        private short myTurn = 0;
        private int myMoney = 0;
        private int otherPlayerBettingMoney = 0;
        private int otherPlayerMoney = 0;
        private int totalBettingMoney = 0;
        

        Thread newGameStart = null;

        //첫번째 턴일때는 하프는 총베팅액기준으로 계산
        private bool isFirstTurn = false;

        #region ButtonEvent
        public UCIndianPoker()
        {
            InitializeComponent();
        }

        private void Button_Call_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            SendBetting(Betting.BettingCall);
        }

        private void Button_Die_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            SendBetting(Betting.BettingDie);
        }

        private void Button_Double_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            SendBetting(Betting.BettingDouble);
        }

        private void Button_Check_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            SendBetting(Betting.BettingCheck);
        }

        private void Button_Queter_Click(object sender, RoutedEventArgs e)
        {
            SetButtonsDisable();
            SendBetting(Betting.BettingQueter);
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
        #endregion

        public void SendBetting(Betting betting)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();

            int bettingMoney = 0;

            switch (betting)
            {
                case Betting.BettingCall:
                    //게임 끝, 새로운 게임 시작
                    bettingMoney = otherPlayerBettingMoney;
                    myMoney = myMoney - bettingMoney;
                    //totalBettingMoney = totalBettingMoney + bettingMoney; //여기가 좀 이해가 안감 검토.
                    break;
                case Betting.BettingDie:
                    //게임 끝, 새로운 게임 시작
                    //Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    //    TextBox_UserLog.AppendText("게임에서 졌습니다. \n");
                    //    TextBox_UserLog.AppendText("새로운 게임을 시작하겠습니다. 준비하세요.\n");
                    //}));

                    //newGameStart = new Thread(new ThreadStart(SendNewGameThread));
                    //newGameStart.Start();
                    break;
                case Betting.BettingDouble:
                    if (isFirstTurn)
                    {
                        isFirstTurn = false;
                    }
                    else
                    {
                        bettingMoney = otherPlayerBettingMoney * 2;
                        myMoney = myMoney - bettingMoney;
                        totalBettingMoney = totalBettingMoney + (otherPlayerBettingMoney * 2);
                    }
                    break;
                case Betting.BettingCheck:
                    if (isFirstTurn)
                    {
                        //첫번째 베팅일땐 체크버튼 활성화
                        isFirstTurn = false;
                    }
                    else
                    {
                        //첫번째 베팅일땐 체크버튼 비활성화
                    }
                    break;
                case Betting.BettingQueter:
                    if (isFirstTurn)
                    {
                        //첫번째 베팅일땐 쿼터버튼 비활성화.
                    }
                    else //총금액의 1/4
                    {
                        bettingMoney = otherPlayerBettingMoney + ((totalBettingMoney + otherPlayerBettingMoney) / 4);
                        myMoney = myMoney - bettingMoney;
                        totalBettingMoney = (totalBettingMoney + otherPlayerBettingMoney) + ((totalBettingMoney + otherPlayerBettingMoney) / 4);
                    }
                    break;
                case Betting.BettingHalf:
                    //첫베팅은 무조건 하프만 됨.
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
                        bettingMoney = otherPlayerBettingMoney + ((totalBettingMoney + otherPlayerBettingMoney) / 2);
                        myMoney = myMoney - bettingMoney;
                        totalBettingMoney = (totalBettingMoney + otherPlayerBettingMoney) + ((totalBettingMoney + otherPlayerBettingMoney) / 2);
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
            if(gamePacketParam.playerTurn == myIndex)
                PrintBetting(betting);

            this.otherPlayerMoney = gamePacketParam.MyMoney;
            this.otherPlayerBettingMoney = gamePacketParam.BettingMoney;
            this.totalBettingMoney = totalBettingMoney + gamePacketParam.BettingMoney;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Label_OtherPlayerMoney.Content = this.otherPlayerMoney.ToString();
                Label_BetTotalMoney.Content = totalBettingMoney.ToString();
            }));

            if (gamePacketParam.Betting == (short)Betting.BettingCall)
            {
                if (gamePacketParam.VictoryUser == myIndex)
                {
                    myMoney = myMoney + totalBettingMoney;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Button_MyCard.Content = this.myCard.ToString();
                        TextBox_UserLog.AppendText("게임에서 이겼습니다. \n");
                        TextBox_UserLog.AppendText("새로운 게임을 시작하겠습니다. 준비하세요.\n");
                        Label_MyMoney.Content = myMoney.ToString();
                    }));
                    
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Button_MyCard.Content = this.myCard.ToString();
                        TextBox_UserLog.AppendText("게임에서 졌습니다. \n");
                        TextBox_UserLog.AppendText("새로운 게임을 시작하겠습니다. 준비하세요.\n");
                    }));
                }
                newGameStart = new Thread(new ThreadStart(SendNewGameThread));
                newGameStart.Start();
            }

            else if (gamePacketParam.Betting == (short)Betting.BettingDie)
            {
                myMoney = myMoney + totalBettingMoney;

                if(gamePacketParam.VictoryUser == myIndex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TextBox_UserLog.AppendText("상대방이 베팅을 포기하여 게임에서 이겼습니다. \n");
                        TextBox_UserLog.AppendText("새로운 게임을 시작하겠습니다. 준비하세요.\n");
                        Label_MyMoney.Content = myMoney.ToString();
                    }));
                }
                else if(gamePacketParam.VictoryUser != myIndex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TextBox_UserLog.AppendText("게임에서 졌습니다. \n");
                        TextBox_UserLog.AppendText("새로운 게임을 시작하겠습니다. 준비하세요.\n");
                    }));
                }
                
                newGameStart = new Thread(new ThreadStart(SendNewGameThread));
                newGameStart.Start();
            }

            else
            {
                
            }
        }

        private void SendNewGameThread()
        {
            Thread.Sleep(5000);
            HandleGamePacket tempHandleGamePacket = new HandleGamePacket();

            tempHandleGamePacket.loadingComplete = true;
            SendNewGameMessage(tempHandleGamePacket);
        }

        public void SetGameStart(HandleGamePacket gamePacketParam)
        {
            if (newGameStart != null)
            {
                newGameStart.Join();
                newGameStart = null;
            }

            //if (this.isPlayGame == false)
            //{
            //    this.isPlayGame = true;
            //}
            //else
            //{
            //    this.myTurn = gamePacketParam.playerTurn;
            //}

            this.myIndex = gamePacketParam.MyIndex;
            this.myTurn = gamePacketParam.playerTurn;
            this.myCard = gamePacketParam.MyCard;
            this.otherPlayerCard = gamePacketParam.OtherPlayerCard;
            this.myMoney = gamePacketParam.MyMoney;
            this.otherPlayerMoney = gamePacketParam.OtherPlayerMoney;
            this.totalBettingMoney = gamePacketParam.TotalBettingMoney;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //Button_MyCard.Content = this.myCard.ToString();
                Button_OtherPlayerCard.Content = this.otherPlayerCard.ToString();

                Label_MyMoney.Content = this.myMoney.ToString();
                Label_OtherPlayerMoney.Content = this.otherPlayerMoney.ToString();

                Label_BetTotalMoney.Content = this.totalBettingMoney.ToString();

                if (myTurn == 1)
                {
                    TextBox_UserLog.AppendText("게임이 시작되었습니다. 선턴입니다. 베팅 하세여 \n");
                    Button_Call.IsEnabled = false;
                    SetButtonsEnable();
                    isFirstTurn = true;
                }
                else
                {
                    TextBox_UserLog.AppendText("게임이 시작되었습니다. 후턴입니다. \n");
                    SetButtonsDisable();
                }
                Button_MyCard.Content = "뒷면";
            }));
        }

        private void PrintBetting(Betting bettingParam)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (bettingParam)
                {
                    case Betting.BettingCall:
                        TextBox_UserLog.AppendText("상대방이 콜 베팅했습니다. \n");
                        break;
                    case Betting.BettingCheck:
                        TextBox_UserLog.AppendText("상대방이 체크 베팅했습니다. \n");
                        break;
                    case Betting.BettingDie:
                        TextBox_UserLog.AppendText("상대방이 다이 베팅했습니다. \n");
                        break;
                    case Betting.BettingDouble:
                        TextBox_UserLog.AppendText("상대방이 따당 베팅했습니다. \n");
                        break;
                    case Betting.BettingQueter:
                        TextBox_UserLog.AppendText("상대방이 쿼터 베팅했습니다. \n");
                        break;
                    case Betting.BettingHalf:
                        TextBox_UserLog.AppendText("상대방이 하프 베팅했습니다. \n");
                        break;
                }
                //IndianPokerScreen.TotalBettingMoney = e.Data.betting;
                SetButtonsEnable();
            }));
        }

        private void PrintGameResult(bool isWin)
        {
            if (isWin)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextBox_UserLog.AppendText("게임에서 이겼습니다. \n");
                    TextBox_UserLog.AppendText("새로운 게임을 시작하겠습니다. 준비하세요.\n");
                    Label_MyMoney.Content = myMoney.ToString();
                }));
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextBox_UserLog.AppendText("게임에서 졌습니다. \n");
                    TextBox_UserLog.AppendText("새로운 게임을 시작하겠습니다. 준비하세요.\n");
                    Label_MyMoney.Content = myMoney.ToString();
                }));
            }

            newGameStart = new Thread(new ThreadStart(SendNewGameThread));
            newGameStart.Start();
        }

        //선턴은 하프,체크만 가능
        public void SetButtonsEnable()
        {
            Button_Call.IsEnabled = true;
            Button_Die.IsEnabled = true;
            Button_Double.IsEnabled = true;
            Button_Check.IsEnabled = true;
            Button_Queter.IsEnabled = true;
            Button_Half.IsEnabled = true;
        }
        
        //후턴부터는 체크제외 가능
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
