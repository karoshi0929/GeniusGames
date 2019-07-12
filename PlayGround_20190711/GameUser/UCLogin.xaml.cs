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
    /// UCLogin.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public delegate void Loginbtn(string message);

    public partial class UCLogin : UserControl
    {

        public event Loginbtn Loginbtn_event;

        public UCLogin()
        {
            InitializeComponent();
        }

        public string IDboxString
        {
            get { return this.IDbox.Text; }
            set { this.IDbox.Text = value; }
        }

        private void Loginbtn_Click(object sender, RoutedEventArgs e)
        {
            Loginbtn_event("SetSelectScreen");
        }
    }
}
