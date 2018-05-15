using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public User CurrentUser;
        public ClientConnector connector;
        public MessageManager manager;

        //构造ClientConnector和MessageManager的实例，并创建登录窗口
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            connector = new ClientConnector();
            manager = new MessageManager();
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}
