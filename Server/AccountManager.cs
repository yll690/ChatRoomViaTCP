using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Net;
using Common;


namespace Server
{
    public class AccountManager
    {
        public event EventHandler<User> UserJoinEvent;
        public event EventHandler<User> UserQuitEvent;
        public event EventHandler<MessageDictionary> MessageArrivedEvent;
        public event EventHandler<string> LogEvent;

        string defaultAccountPath = "Accounts.dat";
        private bool log = true;
        private List<Account> AccountList = new List<Account>();
        private List<UserSocket> LoginedUserList = new List<UserSocket>();
        private ServerConnector connector = ((App)Application.Current).connector;

        public AccountManager()
        {
            LoadAccount();
            connector.LoginEvent += Connector_LoginEvent;
            connector.LogoutEvent += Connector_LogoutEvent;
            connector.SignUpEvent += Connector_SignUpEvent;
            connector.GroupMessageEvent += Connector_GroupMessageEvent;
            connector.PrivateMessageEvent += Connector_PrivateMessageEvent;
            connector.DisconnectEvent += Connector_DisconnectEvent;
            connector.ServerClosingEvent += Connector_ServerClosingEvent;
            log = connector.log;
        }

        private void ShowMessage(string s)
        {
            if (log)
                LogEvent?.Invoke(this, s);
            else
                MessageBox.Show(s);
        }

        private void LoadAccount()
        {
            string[] lines;
            if (File.Exists(defaultAccountPath))
            {
                lines = File.ReadAllLines(defaultAccountPath, Encoding.Default);
                foreach (string line in lines)
                {
                    if (line.Equals("\n"))
                        continue;
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

        private void Connector_GroupMessageEvent(object sender, MessageDictionary e)
        {
            UserSocket user = GetUserSocket(e[MesKeyStr.UserID]);
            string ip = ((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString();
            e.Add(MesKeyStr.NickName, user.NickName);
            e.Add(MesKeyStr.IP, ip);
            e.Add(MesKeyStr.DateTime, DateTime.Now.ToString());
            MessageArrivedEvent?.Invoke(this, e);
            e.Add(MesKeyStr.Sender, Sender.others.ToString());
            foreach (UserSocket u in LoginedUserList)
            {
                if (u.UserID.Equals(e[MesKeyStr.UserID]))
                    continue;
                connector.SendMessage(u.Socket, e);
            }
            e[MesKeyStr.Sender] = Sender.self.ToString();
            connector.SendMessage(user.Socket, e);
        }

        private void Connector_PrivateMessageEvent(object sender, MessageDictionary e)
        {
            for (int i = 0; i < LoginedUserList.Count; i++)
            {
                if (LoginedUserList[i].UserID.Equals(e[MesKeyStr.TargetUserID]))
                {
                    UserSocket user = GetUserSocket(e[MesKeyStr.UserID]);
                                        string ip = ((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString();
                    e.Add(MesKeyStr.NickName, user.NickName);
                    e.Add(MesKeyStr.IP, ip);
                    e.Add(MesKeyStr.DateTime, DateTime.Now.ToString());
                    MessageArrivedEvent?.Invoke(this, e);
                    e.Add(MesKeyStr.Sender, Sender.self.ToString());
                    connector.SendMessage(user.Socket, e);
                    e[MesKeyStr.Sender] = Sender.others.ToString();
                    connector.SendMessage(LoginedUserList[i].Socket, e);
                }
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
                byte[] buffer = Encoding.Default.GetBytes(account.ToString() + "\n");
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
