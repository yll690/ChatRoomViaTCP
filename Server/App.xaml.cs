using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Server
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public ServerConnector connector;
        public AccountManager accountManager;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            connector = new ServerConnector();
            accountManager = new AccountManager();
            SeverChatWindow window = new SeverChatWindow();
            window.Show();
        }
    }
}
