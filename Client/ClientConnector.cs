using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    public class ClientConnector
    {
        public bool isLogined = false;
        public bool isConnected = false;
        
        static char separator = StaticStuff.separator;
        bool listening = true;
        bool log = true;

        Properties.Settings settings = Properties.Settings.Default;
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        int bufferLength = 5 * 1024;
        Thread receiveThread;

        public event EventHandler<MessageDictionary> GroupMessageEvent;
        public event EventHandler<MessageDictionary> PrivateMessageEvent;
        public event EventHandler<bool> LoginEvent;
        public event EventHandler<string> SignupResultEvent;
        public event EventHandler<User> UserJoinEvent;
        public event EventHandler<User> UserQuitEvent;
        public event EventHandler ServerDisconnectEvent;
        public event EventHandler<string> LogEvent;

        public ClientConnector()
        {
            
        }

        void ShowMessage(string s)
        {
            if (log)
                LogEvent?.Invoke(this, s);
            else
                MessageBox.Show(s);
        }

        private void ReceiveFromServer()
        {
            try
            {
                while (listening)
                {
                    byte[] buffer = new byte[bufferLength];
                    int length = clientSocket.Receive(buffer);
                    int lastIndexOfEnd = 0;
                    for(int i=0;i<length;i++)
                        if(buffer[i]=='\0')
                        {
                            MessageSorter(buffer, lastIndexOfEnd, i-1-lastIndexOfEnd);
                            lastIndexOfEnd = i + 1;
                        }
                    //MessageSorter(buffer, 0, length);
                }
            }
            catch (Exception e)
            {
                //if (e.GetType() == typeof(SocketException) && ((SocketException)e).ErrorCode == 10004)
                //    ;
                ShowMessage(e.Message + "\n" + e.StackTrace + "\n");
                Thread.CurrentThread.Abort();
            }
        }

        private void MessageSorter(byte[] buffer, int start, int length)
        {
            string content = Encoding.Default.GetString(buffer, start, length);
            ShowMessage("接收消息：" + content + "\n");
            MessageDictionary messageD = new MessageDictionary(content);
            CommandType command = (CommandType)Enum.Parse(typeof(CommandType), messageD[MesKeyStr.CommandType]);

            switch (command)
            {
                case CommandType.LoginResult:
                    {
                        if (messageD[MesKeyStr.LoginResult].Equals("True"))
                        {
                            ((App)Application.Current).CurrentUser = new User(messageD[MesKeyStr.UserID], messageD[MesKeyStr.NickName]);
                            isLogined = true;
                            LoginEvent?.Invoke(this, true);
                            //UserJoinEvent?.Invoke(this, ((App)Application.Current).user);
                        }
                        else
                        {
                            isLogined = false;
                            LoginEvent?.Invoke(this, false);
                        }
                        break;
                    }
                case CommandType.SignUpResult:
                    {
                        SignupResultEvent?.Invoke(this, messageD[MesKeyStr.UserID]);
                        break;
                    }
                case CommandType.GroupMessage:
                    {
                        GroupMessageEvent?.Invoke(this, messageD);
                        break;
                    }
                case CommandType.PrivateMessage:
                    {
                        PrivateMessageEvent?.Invoke(this, messageD);
                        break;
                    }
                case CommandType.UserJoin:
                    {
                        UserJoinEvent?.Invoke(this, new User(messageD[MesKeyStr.UserID], messageD[MesKeyStr.NickName]));
                        break;
                    }
                case CommandType.UserQuit:
                    {
                        UserQuitEvent?.Invoke(this, new User(messageD[MesKeyStr.UserID], messageD[MesKeyStr.NickName]));
                        break;
                    }
                case CommandType.ServerDisconnect:
                    {
                        ServerDisconnectEvent?.Invoke(this, new EventArgs());
                        Close();
                        break;
                    }
                case CommandType.Remove:
                    {
                        ServerDisconnectEvent?.Invoke(this, new EventArgs());
                        Close();
                        break;
                    }
                case CommandType.Login:
                case CommandType.Logout:
                case CommandType.SignUp:
                    {
                        ShowMessage("收到错误的消息类型！");
                        throw new Exception("收到错误的消息类型！");
                        break;
                    }
            }
        }

        public bool connect()
        {
            IPEndPoint serverPoint = new IPEndPoint(IPAddress.Parse(settings.defaultIP), settings.defaultPort);
            return connect(serverPoint);
        }

        public bool connect(IPEndPoint iPEndPoint)
        {
            if (isConnected)
                return true;
            try
            {
                if (clientSocket == null)
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (clientSocket.Connected == false)
                    clientSocket.Connect(iPEndPoint);
                listening = true;
                receiveThread = new Thread(ReceiveFromServer);
                receiveThread.Start();
                return true;
            }
            catch (Exception e)
            {
                ShowMessage(e.Message + "\n" + e.StackTrace + "\n");
                return false;
            }
        }

        public bool Send(byte[] message, int length)
        {
            try
            {
                clientSocket.Send(message, length, SocketFlags.None);
                return true;
            }
            catch (Exception e)
            {
                ShowMessage(e.Message + "\n" + e.StackTrace + "\n");
                return false;
            }
        }

        public bool Send(string message)
        {
            ShowMessage("发送消息：" + message + "\n");
            message += ";\0";
            byte[] bytes = Encoding.Default.GetBytes(message);
            return Send(bytes, bytes.Length);
        }

        public bool Login(string userID, string password)
        {
            MessageDictionary messageD = new MessageDictionary();
            messageD.Add(MesKeyStr.CommandType, CommandType.Login.ToString());
            messageD.Add(MesKeyStr.UserID,userID);
            messageD.Add(MesKeyStr.PassWord,password);
            try
            {
                if (Send(messageD.ToString(separator)) == false)
                    return false;
                return true;
            }
            catch (Exception e)
            {
                ShowMessage(e.Message + "\n" + e.StackTrace + "\n");
                return false;
            }
        }

        public void Logout()
        {
            User user = ((App)Application.Current).CurrentUser;
            MessageDictionary messageD = new MessageDictionary();
            messageD.Add(MesKeyStr.CommandType, CommandType.Logout.ToString());
            messageD.Add(MesKeyStr.UserID, user.UserID);
            messageD.Add(MesKeyStr.NickName, user.NickName);
            Send(messageD.ToString());
            Close();
        }

        public void SignUp(string nickName, string password)
        {
            MessageDictionary messageD = new MessageDictionary();
            messageD.Add(MesKeyStr.CommandType, CommandType.SignUp.ToString());
            messageD.Add(MesKeyStr.NickName, nickName);
            messageD.Add(MesKeyStr.PassWord, password);
            Send(messageD.ToString());
        }

        public bool SendGroupMessage(MessageDictionary message)
        {
            message.Add(MesKeyStr.CommandType, CommandType.GroupMessage.ToString());
            return Send(message.ToString(separator));
        }

        public bool SendPrivateMessage(MessageDictionary message)
        {
            message.Add(MesKeyStr.CommandType, CommandType.PrivateMessage.ToString());
            return Send(message.ToString(separator));
        }

        public void Close()
        {
            listening = false;
            isLogined = false;
            isConnected = false;
            if (clientSocket != null)
            {
                if (clientSocket.Connected)
                {
                    clientSocket.Disconnect(false);
                    //clientSocket.Shutdown(SocketShutdown.Both);
                }
                clientSocket.Close();
            }
            //if (receiveThread != null && receiveThread.IsAlive)
            //   receiveThread.Abort();
        }

    }
}
