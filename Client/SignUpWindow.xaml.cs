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
using System.Windows.Shapes;
using Common;

namespace Client
{
    /// <summary>
    /// SignUpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SignUpWindow : Window
    {
        public string NickName { get; set; }
        public string Password { get; set; }

        public SignUpWindow()
        {
            InitializeComponent();

        }

        private void signUpB_Click(object sender, RoutedEventArgs e)
        {
            if (nickNameTB.Text.Length == 0 || passwordPB.Password.Length == 0)
            {
                MessageBox.Show(this, "昵称或密码不可为空！");
                return;
            }
            char[] c = { ';', '\0' };
            if (nickNameTB.Text.IndexOfAny(c) >= 0 || passwordPB.Password.IndexOfAny(c) >= 0)
            {
                MessageBox.Show(this, "昵称或密码中不可含有';'和'\0'！");
                return;
            }
            if (nickNameTB.Text.Length > 20 || passwordPB.Password.Length > 20)
            {
                MessageBox.Show(this, "昵称或密码不得超过20字符长！");
                return;
            }
            NickName = nickNameTB.Text;
            Password = StaticStuff.GetMD5(passwordPB.Password);
            DialogResult = true;
            Close();
        }

        private void cancelB_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
