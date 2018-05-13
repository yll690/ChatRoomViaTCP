using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Net;
using Client;

namespace Server
{
    public class AccountManager
    {
        string defaultAccountPath = "Accounts.dat";
        List<Account> AccountList = new List<Account>();
        List<UserSocket> LoginedUserList = new List<UserSocket>();
        ServerConnector connector = ((App)Application.Current).connector;
        bool log = true;

        public event EventHandler<User> UserJoinEvent;
        public event EventHandler<User> UserQuitEvent;
        public event EventHandler<MessageD> MessageArrivedEvent;
        public event EventHandler<string> LogEvent;

        public AccountManager()
        {
            LoadAccount();
            connector.LoginEvent += Connector_LoginEvent;
            connector.LogoutEvent += Connector_LogoutEvent;
            connector.SignUpEvent += Connector_SignUpEvent;
            connector.GroupMessageEvent += Connector_GroupMessageEvent; ;
            connector.DisconnectEvent += Connector_DisconnectEvent;
            connector.ServerClosingEvent += Connector_ServerClosingEvent;
            log = connector.log;
        }

        void ShowMessage(string s)
        {
            if (log)
                LogEvent?.Invoke(this, s);
            else
                MessageBox.Show(s);
        }

        void LoadAccount()
        {
            string[] lines;
            if (File.Exists(defaultAccountPath))
            {
                lines = File.ReadAllLines(defaultAccountPath);
                foreach (string line in lines)
                {
                    AccountList.Add(Account.Parse(line));
                }
            }
        }

        public void RemoveUser(User user)
        {
            connector.SendServerClosingMessage(GetUserSocket(user.UserID).Socket);
            Connector_LogoutEvent(this, user);
        }

        public Account GetAccount(string userID)
        {
            foreach (Account account in AccountList)
            {
                if (userID == account.UserID)
                    return account;
            }
            return null;
        }

        public User GetUser(string userID)
        {
            return GetUserSocket(userID).ToUser();
        }

        public UserSocket GetUserSocket(string userID)
        {
            foreach (UserSocket user in LoginedUserList)
            {
                if (user.UserID.Equals(userID))
                    return user;
            }
            return null;

        }

        //关于connector的事件处理方法
        #region
        private void Connector_ServerClosingEvent(object sender, EventArgs e)
        {
            foreach (UserSocket us in LoginedUserList)
            {
                connector.SendServerClosingMessage(us.Socket);
            }
        }

        private void Connector_DisconnectEvent(object sender, System.Net.Sockets.Socket e)
        {
            foreach (UserSocket us in LoginedUserList)
            {
                if (us.Socket.Equals(e))
                {
                    Connector_LogoutEvent(this, us.ToUser());
                    break;
                }
            }
        }

        private void Connector_LogoutEvent(object sender, User e)
        {
            UserSocket userSocket = GetUserSocket(e.UserID);
            if (userSocket == null)
            {
                ShowMessage("要注销的用户不存在");
                return;
            }
            LoginedUserList.Remove(userSocket);
            UserQuitEvent?.Invoke(this, userSocket.ToUser());
            foreach (UserSocket u in LoginedUserList)
            {
                connector.SendUserChange(u, e, CommandType.UserQuit);
            }
        }

        //private void Connector_GroupMessageEvent(object sender, ChatMessageSend e)
        //{
        //    UserSocket user = GetUserSocket(e.UserID);
        //    string ip = ((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString();
        //    ChatMessage chatMessage = new ChatMessage(e, user.NickName, ip, DateTime.Now.ToString());
        //    MessageArrivedEvent?.Invoke(this, chatMessage);
        //    foreach (UserSocket u in LoginedUserList)
        //    {
        //        connector.SendMessage(u.Socket, chatMessage);
        //    }
        //}
        private void Connector_GroupMessageEvent(object sender, MessageD e)
        {
            UserSocket user = GetUserSocket(e[MesKeyStr.UserID]);
            string ip = ((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString();
            e.Add(MesKeyStr.NickName, user.NickName);
            e.Add(MesKeyStr.IP, ip);
            e.Add(MesKeyStr.DateTime, DateTime.Now.ToString());
            MessageArrivedEvent?.Invoke(this, e);
            foreach (UserSocket u in LoginedUserList)
            {
                connector.SendMessage(u.Socket, e);
            }
        }
        private void Connector_SignUpEvent(object sender, SignUpEventArgs e)
        {
            string userID;
            if (AccountList.Count == 0)
                userID = "10000";
            else
                userID = (Int32.Parse(AccountList[AccountList.Count - 1].UserID) + 1).ToString();
            Account account = new Account(userID, e.Password, e.NickName);
            AccountList.Add(account);
            connector.SendSignUpResult(e.SignUpSocket, userID);
            try
            {
                FileStream fileStream = File.Open(defaultAccountPath, FileMode.Append);
                byte[] buffer = Encoding.Default.GetBytes("\n" + account.ToString());
                fileStream.Write(buffer, 0, buffer.Length);
                fileStream.Close();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message + "\n" + ex.StackTrace + "\n");
            }
        }

        private void Connector_LoginEvent(object sender, LoginEventArgs e)
        {
            Account account = GetAccount(e.UserID);
            if (account == null)
            {
                connector.SendLoginResult(new UserSocket(e.UserID, " ", e.ReceiveSocket), false);
                return ;
            }

            UserSocket newUserSocket = new UserSocket(account.UserID, account.NickName, e.ReceiveSocket);
            if (account.ConfirmPassword(e.PassWord) && GetUserSocket(e.UserID) == null)
            {
                connector.SendLoginResult(newUserSocket, true);
                //connector.SendUserChange(newUserSocket, newUserSocket.ToUser(), CommandType.UserJoin);

                //向已登录的用户发送新登录用户的信息
                foreach (UserSocket oldUser in LoginedUserList)
                {
                    connector.SendUserChange(oldUser, newUserSocket.ToUser(), CommandType.UserJoin);
                }

                //向新登录用户发送已登录的用户的信息
                foreach (UserSocket oldUser in LoginedUserList)
                {
                    connector.SendUserChange(newUserSocket, oldUser.ToUser(), CommandType.UserJoin);
                }

                LoginedUserList.Add(newUserSocket);

                UserJoinEvent?.Invoke(this, new User(newUserSocket.UserID, newUserSocket.NickName));
                return;
            }
            else
            {
                connector.SendLoginResult(newUserSocket, false);
                return;
            }
        }
        #endregion
    }
}
