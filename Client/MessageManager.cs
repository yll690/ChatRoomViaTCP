using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Common;

namespace Client
{
    public class MessageManager
    {
        ClientConnector connector = ((App)Application.Current).connector;
        ChatWindow GroupChatWindow;
        List<ChatWindow> privateWindows = new List<ChatWindow>();
        User currentUser;

        public MessageManager()
        {
            connector.GroupMessageEvent += Connector_GroupMessageEvent;
            connector.PrivateMessageEvent += Connector_PrivateMessageEvent;
            connector.ServerDisconnectEvent += Connector_ServerDisconnectEvent;
            connector.UserJoinEvent += Connector_UserJoinEvent;
            connector.UserQuitEvent += Connector_UserQuitEvent;
        }

        public void StartChatting()
        {
            currentUser = ((App)Application.Current).CurrentUser;
            GroupChatWindow = new ChatWindow();
            GroupChatWindow.PrivateChatEvent += GroupChatWindow_PrivateChatEvent;
            GroupChatWindow.Closed += GroupChatWindow_Closed;
            GroupChatWindow.Show();
        }

        private void GroupChatWindow_PrivateChatEvent(object sender, User e)
        {
            ChatWindow privateChatWindow = new ChatWindow(e);
            privateWindows.Add(privateChatWindow);
            privateChatWindow.Show();
        }

        private void GroupChatWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (ChatWindow cw in privateWindows)
                    cw.Close();
            });
            connector.Close();
        }
        
        private void ChatWindow_ManualCloseEvent(object sender, EventArgs e)
        {
            ChatWindow chatWindow = (ChatWindow)sender;
            privateWindows.Remove(chatWindow);
        }

        //关于connector的事件处理方法
        #region
        private void Connector_UserQuitEvent(object sender, User e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GroupChatWindow.UserQuit(e);
            });
            foreach (ChatWindow cw in privateWindows)
                if (cw.TargetUser.UserID.Equals(e.UserID))
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        cw.TargetQuit(e);
                    });
        }

        private void Connector_UserJoinEvent(object sender, User e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GroupChatWindow.UserJoin(e);
            });
        }

        private void Connector_ServerDisconnectEvent(object sender, EventArgs e)
        {
            MessageBox.Show("服务器关闭或失去连接，请重新登录。");
            LoginWindow loginWindow = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                loginWindow = new LoginWindow();
                loginWindow.Show();
                GroupChatWindow.Close();
            });
        }

        private void Connector_PrivateMessageEvent(object sender, MessageDictionary e)
        {
            bool found = false;
            Sender s = (Sender)Enum.Parse(typeof(Sender), e[MesKeyStr.Sender]);
            string targetUserID = s == Sender.others ? e[MesKeyStr.UserID] : e[MesKeyStr.TargetUserID];
            foreach (ChatWindow cw in privateWindows)
            {

                if (cw.TargetUser.UserID.Equals(targetUserID))
                {
                    found = true;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        cw.MessageArrive(e);
                    });
                }
            }
            if (found == false)
            {
                User target = new User(e[MesKeyStr.UserID], e[MesKeyStr.NickName]);
                ChatWindow chatWindow = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chatWindow = new ChatWindow(target);
                    chatWindow.ManualCloseEvent += ChatWindow_ManualCloseEvent;
                    chatWindow.Show();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        chatWindow.MessageArrive(e);
                    });
                });
                privateWindows.Add(chatWindow);
            }
        }

        private void Connector_GroupMessageEvent(object sender, MessageDictionary e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GroupChatWindow.MessageArrive(e);
            });
        }
        #endregion
    }
}
