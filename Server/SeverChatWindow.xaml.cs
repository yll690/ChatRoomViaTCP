using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Common;

namespace Server
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SeverChatWindow : Window
    {
        private bool displayStyle = false;
        private int maxMesListLen = StaticStuff.MaxMesListLen;

        private ServerConnector connector = ((App)Application.Current).connector;
        private AccountManager manager = ((App)Application.Current).accountManager;
        private ObservableCollection<User> userList = new ObservableCollection<User>();

        public SeverChatWindow()
        {
            InitializeComponent();
            userListLV.ItemsSource = userList;
            portTB.Text = connector.Port.ToString();
            portTB.ToolTip = connector.Port.ToString();

            manager.UserJoinEvent += Manager_UserJoinEvent;
            manager.UserQuitEvent += Manager_UserQuitEvent;
            manager.MessageArrivedEvent += Manager_MessageArrivedEvent;
            manager.LogEvent += Manager_LogEvent;
            connector.LogEvent += Connector_LogEvent;
        }

        //关于窗口事件的事件处理方法
        #region
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connector.Close();
        }

        private void removeUser_Click(object sender, RoutedEventArgs e)
        {
            manager.RemoveUser(userList[userListLV.SelectedIndex]);
        }

        private void disPlayStyleCB_Click(object sender, RoutedEventArgs e)
        {
            if (displayStyle)
                displayStyle = false;
            else
                displayStyle = true;
        }

        private void displayLogCB_Click(object sender, RoutedEventArgs e)
        {
            if (displayLogCB.IsChecked == false)
            {
                logTB.Visibility = Visibility.Collapsed;
                logGS.Visibility = Visibility.Collapsed;
                mainGrid.ColumnDefinitions[4].Width = new GridLength(0);
            }
            else
            {
                logTB.Visibility = Visibility.Visible;
                logGS.Visibility = Visibility.Visible;
                mainGrid.ColumnDefinitions[4].Width = new GridLength(5, GridUnitType.Star);
            }
        }
        #endregion

        private void Log(string log)
        {
            int index = log.IndexOf(StaticStuff.Separator + MesKeyStr.Base64String + ":");
            if (index >= 0)
            {
                int index2 = log.IndexOf(StaticStuff.Separator, index + 1);
                string newLog = log.Substring(0, index) + "[图片]" + log.Substring(index2);
                logTB.Text += newLog + "\n";
            }
            else
                logTB.Text += log + "\n";
            logTB.ScrollToEnd();
        }

        private void changePortB_Click(object sender, RoutedEventArgs e)
        {
            portTB.IsReadOnly = !portTB.IsReadOnly;
            if (portTB.IsReadOnly == true)
            {
                int port;
                try
                {
                    port = Convert.ToInt32(portTB.Text);
                    if (port < 0 || port > 65535)
                        throw new ArgumentOutOfRangeException(nameof(port), port, "输入的值应该在0-65535之间！");
                    if (port != Properties.Settings.Default.defaultPort)
                    {
                        Properties.Settings.Default.defaultPort = port;
                        Properties.Settings.Default.Save();
                        if (port != connector.Port)
                            portTB.ToolTip = "目前端口仍是" + connector.Port + "，下次启动时，将变为" + port;
                        else
                            portTB.ToolTip = connector.Port.ToString();
                        MessageBox.Show("修改成功，请重启服务器以应用修改。");
                    }
                    portTB.Text = connector.Port.ToString();
                    changePortB.Content = "修改";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请输入正确的端口！\n" + ex.Message);
                    portTB.IsReadOnly = !portTB.IsReadOnly;
                }
            }
            else
            {
                changePortB.Content = "确认";
                portTB.Focus();
                portTB.SelectAll();
            }
        }

        private void portTB_KeyUp(object sender, KeyEventArgs e)
        {
            if (portTB.IsReadOnly == false && e.Key == Key.Enter)
            {
                portTB.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                changePortB_Click(this, new RoutedEventArgs());
            }
        }

        //关于manager和connector的事件处理方法
        #region
        private void Connector_LogEvent(object sender, string e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Log(e);
            });
        }

        private void Manager_LogEvent(object sender, string e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Log(e);
            });
        }

        private void Manager_MessageArrivedEvent(object sender, MessageDictionary e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (messageListSP.Children.Count >= maxMesListLen)
                    messageListSP.Children.RemoveAt(0);
                if ((CommandType)Enum.Parse(typeof(CommandType), e[MesKeyStr.CommandType]) == CommandType.GroupMessage)
                    e.Add(MesKeyStr.Remark, "群聊消息");
                else
                    e.Add(MesKeyStr.Remark, "私聊消息，目标："+e[MesKeyStr.UserID]);

                DisplayMethod displayMethod=DisplayMethod.OnlyRemark;
                if (displayStyle)
                    displayMethod = DisplayMethod.Both;
                MessageUC messageUC = new MessageUC(e, displayMethod);
                messageListSP.Children.Add(messageUC);
                messageListSV.ScrollToEnd();
            });
        }

        private void Manager_UserQuitEvent(object sender, User e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                userList.Remove(e);
                Title = "目前在线 " + userList.Count + " 人";
                //userListLV.Items.Remove(e.NickName + "(" + e.UserID + ")");
            });
        }

        private void Manager_UserJoinEvent(object sender, User e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                userList.Add(e);
                Title = "目前在线 " + userList.Count + " 人";
                //userListLV.Items.Add(e.NickName + "(" + e.UserID + ")");
            });
        }
        #endregion
    }
}
