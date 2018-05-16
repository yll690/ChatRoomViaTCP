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
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private ClientConnector connector = ((App)Application.Current).connector;
        private MessageManager manager = ((App)Application.Current).manager;
        private Properties.Settings settings = Properties.Settings.Default;

        public LoginWindow()
        {
            InitializeComponent();
            if (settings.userID.Length > 0)
                userIDTB.Text = settings.userID;
            connector.LoginEvent += LoginState;
            connector.SignupResultEvent += Connector_SignupResultEvent;
        }

        private void Connector_SignupResultEvent(object sender, string e)
        {
            MessageBox.Show("注册成功！您的用户ID为：\n" + e + "\n请牢记此ID，并用于登录", "注册结果");
        }

        private void LoginState(object sender, bool isSucceeded)
        {
            if (isSucceeded)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    manager.StartChatting();
                    settings.userID = userIDTB.Text;
                    settings.Save();
                    Close();
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    loginB.IsEnabled = true;
                    loginB.Content = "登录";
                    MessageBox.Show(this, "登陆失败，账号或密码错误，或此账号已登录。");
                });
            }
        }

        private void loginB_Click(object sender, RoutedEventArgs e)
        {
            if (userIDTB.Text.Length == 0 || passwordPB.Password.Length == 0)
            {
                MessageBox.Show("用户ID和密码不能为空");
                return;
            }
            loginB.IsEnabled = false;
            loginB.Content = "登录中...";
            if (!(connector.connect() && connector.Login(userIDTB.Text, StaticStuff.GetMD5(passwordPB.Password))))
            {
                MessageBox.Show(this, "登录失败，无法连接服务器！");
                loginB.IsEnabled = true;
                loginB.Content = "登录";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connector.IsLogined == false)
                connector.Close();
        }

        private void loginSettingL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoginSettingW loginSetting = new LoginSettingW();
            if (loginSetting.ShowDialog() == true)
            {
                settings.defaultIP = loginSetting.IP;
                settings.defaultPort = loginSetting.Port;
                settings.Save();
            }
        }

        private void signUpL_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SignUpWindow signUpWindow = new SignUpWindow();
            signUpWindow.Owner = this;
            if (signUpWindow.ShowDialog() == true)
            {
                if (!connector.connect())
                {
                    MessageBox.Show(this, "注册失败，无法连接服务器！");
                    return;
                }
                connector.SignUp(signUpWindow.NickName, StaticStuff.GetMD5(signUpWindow.Password));
            }
        }
    }
}
