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

namespace Client
{
    /// <summary>
    /// LoginSettingW.xaml 的交互逻辑
    /// </summary>
    public partial class LoginSettingW : Window
    {
        public string IP { get; set; }
        public int Port { get; set; }
        Properties.Settings settings = Properties.Settings.Default;

        public LoginSettingW()
        {
            InitializeComponent();
            ipTB.Text = settings.defaultIP;
            portTB.Text = settings.defaultPort.ToString();
        }

        private void cancelB_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void okB_Click(object sender, RoutedEventArgs e)
        {
            IP = ipTB.Text;
            Port = int.Parse(portTB.Text);
            DialogResult = true;
            Close();
        }
    }
}
