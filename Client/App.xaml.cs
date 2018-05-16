using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Common;

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
        public LogWindow logWindow;

        private bool dislayLogWindow = false;

        public App():base()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
            {
                String resourceName = "Client." + new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }
        
        //构造ClientConnector和MessageManager的实例，并创建登录窗口
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (dislayLogWindow)
            {
                logWindow = new LogWindow();
                logWindow.Show();
            }
            connector = new ClientConnector();
            manager = new MessageManager();
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}
