﻿using System;
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
    /// UCSelectedGame.xaml에 대한 상호 작용 논리
    /// </summary>
    
    public delegate void indianbtn(string message);
    public delegate void mazebtn(string message);
    public delegate void CloseGame();

    public partial class UCSelectedGame : UserControl
    {
        public event indianbtn indianbtn_event;
        public event mazebtn mazebtn_event;
        public event CloseGame CloseGameEvent;
        
        public UCSelectedGame()
        {
            InitializeComponent();
        }

        private void indian_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Label_PrintMessage.Content = "상대방을 찾는 중입니다...";
                Button_StartMatching.IsEnabled = false;
            }));
            indianbtn_event("Set Indian Poker Screen");
        }

        private void maze_Click(object sender, RoutedEventArgs e)
        {
            mazebtn_event("Set Maze of Memory Screen");
        }

        private void Button_ExitGame_Click(object sender, RoutedEventArgs e)
        {
            CloseGameEvent();
        }
    }
}
