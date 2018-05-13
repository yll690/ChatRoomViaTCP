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

        public event EventHandler<MessageD> GroupMessageEvent;
        public event EventHandler<MessageD> PrivateMessageEvent;
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

        private string[] RemoveCommand(string[] contents)
        {
            int length = contents.Length;
            string[] infos = new string[length - 1];
            for (int i = 1; i < length; i++)
                infos[i - 1] = contents[i];
            return infos;
        }

        //private void MessageSorter(byte[] buffer, int start, int length)
        //{
        //    string content = Encoding.Default.GetString(buffer, start, length);
        //    ShowMessage("接收消息：" + content + "\n");
        //    string[] contents = content.Split(separator);
        //    CommandType command = (CommandType)contents[0][0];

        //    switch (command)
        //    {
        //        case CommandType.LoginResult:
        //            {
        //                if (contents[1][0] == 'T')
        //                {
        //                    ((App)Application.Current).user = new User(contents[2], contents[3]);
        //                    isLogined = true;
        //                    LoginEvent?.Invoke(this, true);
        //                    //UserJoinEvent?.Invoke(this, ((App)Application.Current).user);
        //                }
        //                else
        //                {
        //                    isLogined = false;
        //                    LoginEvent?.Invoke(this, false);
        //                }
        //                break;
        //            }
        //        case CommandType.SignUpResult:
        //            {
        //                SignupResultEvent?.Invoke(this, contents[1]);
        //                break;
        //            }
        //        case CommandType.GroupMessage:
        //            {
        //                GroupMessageEvent?.Invoke(this, ChatMessage.Parse(RemoveCommand(contents)));
        //                break;
        //            }
        //        case CommandType.PrivateMessage:
        //            {

        //                break;
        //            }
        //        case CommandType.UserJoin:
        //            {
        //                UserJoinEvent?.Invoke(this, User.Parse(RemoveCommand(contents)));
        //                break;
        //            }
        //        case CommandType.UserQuit:
        //            {
        //                UserQuitEvent?.Invoke(this, User.Parse(RemoveCommand(contents)));
        //                break;
        //            }
        //        case CommandType.ServerDisconnect:
        //            {
        //                ServerDisconnectEvent?.Invoke(this, new EventArgs());
        //                Close();
        //                break;
        //            }
        //        case CommandType.Remove:
        //            {
        //                ServerDisconnectEvent?.Invoke(this, new EventArgs());
        //                Close();
        //                break;
        //            }
        //        case CommandType.Login:
        //        case CommandType.Logout:
        //        case CommandType.SignUp:
        //            {
        //                ShowMessage("收到错误的消息类型！");
        //                throw new Exception("收到错误的消息类型！");
        //                break;
        //            }
        //    }
        //}
        private void MessageSorter(byte[] buffer, int start, int length)
        {
            string content = Encoding.Default.GetString(buffer, start, length);
            ShowMessage("接收消息：" + content + "\n");
            MessageD messageD = new MessageD(content);
            CommandType command = (CommandType)Enum.Parse(typeof(CommandType), messageD["CommandType"]);

            switch (command)
            {
                case CommandType.LoginResult:
                    {
                        if (messageD["LoginResult"].Equals("True"))
                        {
                            ((App)Application.Current).user = new User(messageD["UserID"], ["NickName"]);
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
                        SignupResultEvent?.Invoke(this, messageD["UserID"]);
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
                        UserJoinEvent?.Invoke(this, new User(messageD["UserID"], messageD["NickName"]));
                        break;
                    }
                case CommandType.UserQuit:
                    {
                        UserQuitEvent?.Invoke(this, new User(messageD["UserID"], messageD["NickName"]));
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
            StringBuilder sb = new StringBuilder();
            sb.Append((char)CommandType.Login);
            sb.Append(separator);
            sb.Append(userID);
            sb.Append(separator);
            sb.Append(password);
            try
            {
                if (Send(sb.ToString()) == false)
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
            User user = ((App)Application.Current).user;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append((char)CommandType.Logout);
            stringBuilder.Append(separator);
            stringBuilder.Append(user.UserID);
            stringBuilder.Append(separator);
            stringBuilder.Append(user.NickName);
            Send(stringBuilder.ToString());
            Close();
        }

        public void SignUp(string nickName, string password)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append((char)CommandType.SignUp);
            stringBuilder.Append(separator);
            stringBuilder.Append(nickName);
            stringBuilder.Append(separator);
            stringBuilder.Append(password);
            Send(stringBuilder.ToString());
        }

        //public bool SendMessage(ChatMessageSend message)
        //{
        //    return Send((char)CommandType.GroupMessage + "" + separator + message.ToString(separator));
        //}

        public bool SendMessage(MessageD message)
        {
            message.Add("CommandType", CommandType.GroupMessage.ToString());
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
