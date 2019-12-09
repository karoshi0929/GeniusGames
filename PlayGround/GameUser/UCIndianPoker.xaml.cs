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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GameUser
{
    /// <summary>
    /// UCIndianPoker.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UCIndianPoker : UserControl, INotifyPropertyChanged
    {
        public event Loginbtn CloseButtonEvent;

        public delegate void DelegateSendGameBettingMessage(IndianPokerGamePacket gamePacket);
        public DelegateSendGameBettingMessage SendGamePacketMessage;

        public delegate void DelegateSendNewGameStartMessage(HandleGamePacket handleGamePacket);
        public DelegateSendNewGameStartMessage SendNewGameMessage;

        Thread newGameStart = null;

        private short myCard = 0;
        private short otherPlayerCard = 0;
        private short myIndex = 0;
        private short myTurn = 0;
        private int myMoney = 0;
        private int otherPlayerBettingMoney = 0;
        private int otherPlayerMoney = 0;
        private int totalBettingMoney = 0;

        public bool IsExitGame = false;
        //첫번째 턴일때는 하프는 총베팅액기준으로 계산
        private bool isFirstTurn = false;
        private bool isPlayGame;
        public bool IsPlayGame
        {
            get
            {
                return isPlayGame;
            }
            set
            {
                isPlayGame = !value;
                OnPropertyChanged("IsPlayGame");
            }
        }

        private string strMyBetting;
        public string StrMyBetting
        {
            get
            {
                return strMyBetting;
            }
            set
            {
                strMyBetting = value;
                OnPropertyChanged("StrMyBetting");
            }
        }

        private string strOtherPlayerBetting;
        public string StrOtherPlayerBetting
        {
            get
            {
                return strOtherPlayerBetting;
            }
            set
            {
                strOtherPlayerBetting = value;
                OnPropertyChanged("StrOtherPlayerBetting");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void SetGameStart(HandleGamePacket gamePacketParam)
        {
            StrMyBetting = "";
            StrOtherPlayerBetting = "";

            IsExitGame = false; //나가기 버튼 이벤트 발생 후, 다른 매칭상대와 게임을 시작 할경우 초기화
            IsPlayGame = true; //나가기 버튼 비활성화

            if (newGameStart != null)
            {
                newGameStart.Join();
                newGameStart = null;
            }

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
                    isFirstTurn = true;
                    SetButtonsEnable(isFirstTurn);
                }
                else
                {
                    TextBox_UserLog.AppendText("게임이 시작되었습니다. 후턴입니다. \n");
                    SetButtonsDisable();
                }
                Button_MyCard.Content = "뒷면";
            }));
        }

        #region ButtonEvent
        public UCIndianPoker()
        {
            InitializeComponent();
            this.DataContext = this;
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
        }
        #endregion

        public void SendBetting(Betting bettingParam)
        {
            IndianPokerGamePacket gamePacket = new IndianPokerGamePacket();

            int bettingMoney = 0;
            short betting = (short)bettingParam;

            switch (bettingParam)
            {
                case Betting.BettingCall:
                    //게임 끝, 새로운 게임 시작
                    bettingMoney = otherPlayerBettingMoney;

                    if(this.myMoney - bettingMoney < 0)
                    {
                        bettingMoney = this.myMoney;
                        this.myMoney = 0;
                        this.totalBettingMoney = this.totalBettingMoney + bettingMoney;
                    }
                    else
                    {
                        this.myMoney = myMoney - bettingMoney;
                        this.totalBettingMoney = totalBettingMoney + bettingMoney;
                    }
                    this.StrMyBetting = "콜";
                    break;

                case Betting.BettingDie:
                    this.StrMyBetting = "다이";
                    break;

                case Betting.BettingDouble:
                    bettingMoney = otherPlayerBettingMoney * 2;

                    if (this.myMoney - bettingMoney < 0)
                    {
                        bettingMoney = this.myMoney;
                        this.myMoney = 0;
                        this.totalBettingMoney = this.totalBettingMoney + bettingMoney;
                        betting = (short)Betting.BettingCall;
                        this.StrMyBetting = "콜";
                    }
                    else
                    {
                        this.myMoney = this.myMoney - bettingMoney;
                        this.totalBettingMoney = this.totalBettingMoney + (this.otherPlayerBettingMoney * 2);
                        this.StrMyBetting = "따당";
                    }
                    break;

                case Betting.BettingCheck:
                    if (this.isFirstTurn)
                    {
                        //첫번째 베팅일땐 체크버튼 활성화
                        this.isFirstTurn = false;
                    }
                    else
                    {
                        //첫번째 베팅일땐 체크버튼 비활성화
                    }
                    this.StrMyBetting = "체크";
                    break;

                case Betting.BettingQueter:
                    bettingMoney = otherPlayerBettingMoney + ((totalBettingMoney + otherPlayerBettingMoney) / 4);

                    if(this.myMoney - bettingMoney < 0)
                    {
                        bettingMoney = this.myMoney;
                        this.myMoney = 0;
                        this.totalBettingMoney = this.totalBettingMoney + bettingMoney;
                        betting = (short)Betting.BettingCall;
                        this.StrMyBetting = "콜";
                    }
                    else
                    {
                        this.myMoney = this.myMoney - bettingMoney;
                        this.StrMyBetting = "쿼터";
                    }
                    this.totalBettingMoney = (this.totalBettingMoney + this.otherPlayerBettingMoney) + ((this.totalBettingMoney + this.otherPlayerBettingMoney) / 4);
                    break;

                case Betting.BettingHalf:
                    //첫베팅은 무조건 하프만 됨.
                    if (this.isFirstTurn)
                    {
                        this.isFirstTurn = false;

                        bettingMoney = totalBettingMoney / 2;
                        this.myMoney = this.myMoney - bettingMoney;
                        this.totalBettingMoney = this.totalBettingMoney + bettingMoney;
                    }
                    else
                    {
                        //총베팅금액 + 상대방의 베팅금액 / 2
                        bettingMoney = otherPlayerBettingMoney + ((totalBettingMoney + otherPlayerBettingMoney) / 2);

                        if (this.myMoney - bettingMoney < 0)
                        {
                            bettingMoney = this.myMoney;
                            this.myMoney = 0;
                            this.totalBettingMoney = this.totalBettingMoney + bettingMoney;
                            betting = (short)Betting.BettingCall;
                            this.StrMyBetting = "콜";
                        }
                        else
                        {
                            this.myMoney = this.myMoney - bettingMoney;
                            this.StrMyBetting = "하프";
                        }
                        this.totalBettingMoney = (this.totalBettingMoney + this.otherPlayerBettingMoney) + ((this.totalBettingMoney + this.otherPlayerBettingMoney) / 2);
                    }
                    break;
            }

            StrOtherPlayerBetting = string.Empty;

            gamePacket.playerTurn = myTurn;
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
            if(gamePacketParam.playerTurn == myTurn)
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
                        TextBox_UserLog.AppendText("게임에서 이겼습니다. \n");
                        TextBox_UserLog.AppendText("새로운 게임을 시작하겠습니다. 준비하세요.\n");
                        Button_MyCard.Content = this.myCard.ToString();
                    }));
                }
                else
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        TextBox_UserLog.AppendText("게임에서 졌습니다. \n");
                        TextBox_UserLog.AppendText("새로운 게임을 시작하겠습니다. 준비하세요.\n");
                        Button_MyCard.Content = this.myCard.ToString();
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
                StrMyBetting = string.Empty;
            }
        }

        private void SendNewGameThread()
        {
            this.IsPlayGame = false;
            Thread.Sleep(5000);

            if (myMoney <= 0)
                Button_ExitGameRoom_Click(null, null);

            if (!IsExitGame)
            {
                HandleGamePacket tempHandleGamePacket = new HandleGamePacket();
                tempHandleGamePacket.loadingComplete = true;
                SendNewGameMessage(tempHandleGamePacket);
            }
        }

        private void PrintBetting(Betting bettingParam)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (bettingParam)
                {
                    case Betting.BettingCall:
                        TextBox_UserLog.AppendText("상대방이 콜 베팅했습니다. \n");
                        StrOtherPlayerBetting = "콜";
                        break;
                    case Betting.BettingCheck:
                        TextBox_UserLog.AppendText("상대방이 체크 베팅했습니다. \n");
                        StrOtherPlayerBetting = "체크";
                        break;
                    case Betting.BettingDie:
                        TextBox_UserLog.AppendText("상대방이 다이 베팅했습니다. \n");
                        StrOtherPlayerBetting = "다이";
                        break;
                    case Betting.BettingDouble:
                        TextBox_UserLog.AppendText("상대방이 따당 베팅했습니다. \n");
                        StrOtherPlayerBetting = "따당";
                        break;
                    case Betting.BettingQueter:
                        TextBox_UserLog.AppendText("상대방이 쿼터 베팅했습니다. \n");
                        StrOtherPlayerBetting = "쿼터";
                        break;
                    case Betting.BettingHalf:
                        TextBox_UserLog.AppendText("상대방이 하프 베팅했습니다. \n");
                        StrOtherPlayerBetting = "하프";
                        break;
                }

                if(bettingParam == Betting.BettingCheck || bettingParam == Betting.BettingDouble ||
                   bettingParam == Betting.BettingQueter || bettingParam == Betting.BettingHalf)
                    SetButtonsEnable(false);
            }));
        }

        private void Button_ExitGameRoom_Click(object sender, RoutedEventArgs e)
        {
            HandleGamePacket handleGamePacket = new HandleGamePacket();
            handleGamePacket.startGame = false;

            SendNewGameMessage(handleGamePacket);
            IsExitGame = true;
            CloseButtonEvent("SetSelectScreen");
        }

        //선턴은 하프,체크만 가능
        public void SetButtonsEnable(bool isFirst)
        {
            if(isFirst)
            {
                Button_Call.IsEnabled = false;
                Button_Die.IsEnabled = true;
                Button_Double.IsEnabled = false;
                Button_Check.IsEnabled = true;
                Button_Queter.IsEnabled = false;
                Button_Half.IsEnabled = true;
            }
            else
            {
                Button_Call.IsEnabled = true;
                Button_Die.IsEnabled = true;
                Button_Double.IsEnabled = true;
                Button_Check.IsEnabled = false;
                Button_Queter.IsEnabled = true;
                Button_Half.IsEnabled = true;
            }
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
