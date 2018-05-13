using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Client;

namespace Server
{
    public class ServerConnector
    {
        int defaultPort = 10000;
        int bufferLength = 5 * 1024 * 1024;
        static char separator = StaticStuff.separator;
        bool listening = true;
        public bool log = true;

        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        public delegate void PrivateMessageEH(object sender, string targetUserIP, Socket socket);

        //public event EventHandler<ChatMessageSend> GroupMessageEvent;
        public event EventHandler<MessageD> GroupMessageEvent;
        public event EventHandler<MessageD> PrivateMessageEvent;
        public event EventHandler<LoginEventArgs> LoginEvent;
        public event EventHandler<SignUpEventArgs> SignUpEvent;
        public event EventHandler<User> LogoutEvent;
        public event EventHandler<string> LogEvent;
        public event EventHandler<Socket> DisconnectEvent;
        public event EventHandler ServerClosingEvent;


        public ServerConnector()
        {
            //IPAddress ip = IPAddress.Parse(defaultIP);
            //serverSocket.Bind(new IPEndPoint(ip, defaultPort));
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, defaultPort));
            serverSocket.Listen(10);
            Thread listenThread = new Thread(ListenFromClient);
            listenThread.Start();
        }

        void ShowMessage(string s)
        {
            if (log)
                LogEvent?.Invoke(this, s);
            else
                MessageBox.Show(s);
        }
        
        private void ListenFromClient()
        {
            try
            {
                while (listening)
                {
                    Socket clientSocket = serverSocket.Accept();
                    Thread receiveThread = new Thread(ReceiveMessage);
                    receiveThread.Start(clientSocket);
                }
            }
            catch (Exception e)
            {
                //if (e.GetType() == typeof(SocketException) && ((SocketException)e).ErrorCode == 10004)
                //    ;
                //else
                //{
                    ShowMessage(e.Message + "\n" + e.StackTrace + "\n");
                //}
                //Thread.CurrentThread.Abort();
            }
        }

        private void ReceiveMessage(object socket)
        {
            Socket receiveSocket = (Socket)socket;
            byte[] buffer = new byte[bufferLength];
            while (listening)
            {
                try
                {
                    int length = receiveSocket.Receive(buffer);
                    if (length == 0)
                    {
                        DisconnectEvent?.Invoke(this, receiveSocket);
                        break;

                    }
                    else
                    {
                        int lastIndexOfEnd = 0;
                        for (int i = 0; i < length; i++)
                            if (buffer[i] == '\0')
                            {
                                MessageSorter(buffer, lastIndexOfEnd, i - 1 - lastIndexOfEnd,receiveSocket);
                                lastIndexOfEnd = i + 1;
                            }
                    }
                }
                catch(Exception e)
                {
                    ShowMessage(e.Message + "\n" + e.StackTrace + "\n");
                    //Thread.CurrentThread.Abort();
                }
            }
            ShowMessage("接收"+ receiveSocket.RemoteEndPoint.ToString()+ "的线程已结束\n");
        }

        private string[] RemoveCommand(string[] contents)
        {
            int length = contents.Length;
            string[] infos = new string[length - 1];
            for (int i = 1; i < length; i++)
                infos[i - 1] = contents[i];
            return infos;
        }

        //private void MessageSorter(byte[] buffer,int start, int length, Socket clientSocket)
        //{
        //    string content = Encoding.Default.GetString(buffer, 0, length);
        //    ShowMessage("从" + clientSocket.RemoteEndPoint.ToString() + "接收消息：" + content + "\n");
        //    string[] contents = content.Split(separator);
        //    CommandType command = (CommandType)contents[0][0];

        //    switch (command)
        //    {
        //        case CommandType.Login:
        //            {
        //                LoginEvent?.Invoke(this, new LoginEventArgs()
        //                {
        //                    UserID = contents[1],
        //                    PassWord = contents[2],
        //                    ReceiveSocket = clientSocket
        //                });

        //                break;
        //            }
        //        case CommandType.Logout:
        //            {
        //                LogoutEvent?.Invoke(this, new User(contents[1], contents[2]));
        //                break;
        //            }
        //        case CommandType.SignUp:
        //            {
        //                SignUpEvent?.Invoke(this, new SignUpEventArgs(clientSocket, contents[1], contents[2]));
        //                break;
        //            }
        //        case CommandType.GroupMessage:
        //            {
        //                GroupMessageEvent?.Invoke(this, ChatMessageSend.Parse(RemoveCommand(contents)));
        //                break;
        //            }
        //        case CommandType.PrivateMessage:
        //            {
        //                PrivateMessageEvent?.Invoke(this, ChatMessageSend.Parse(RemoveCommand(contents)));
        //                break;
        //            }
        //        case CommandType.UserJoin:
        //        case CommandType.UserQuit:
        //        case CommandType.LoginResult:
        //        case CommandType.SignUpResult:
        //        case CommandType.ServerDisconnect:
        //        case CommandType.Remove:
        //            {
        //                ShowMessage("收到错误的消息类型！");
        //                throw new Exception("收到错误的消息类型！");
        //            }

        //    }
        //}

        private void MessageSorter(byte[] buffer, int start, int length, Socket clientSocket)
        {
            string content = Encoding.Default.GetString(buffer, 0, length);
            ShowMessage("从" + clientSocket.RemoteEndPoint.ToString() + "接收消息：" + content + "\n");
            MessageD messageD = new MessageD(content);
            CommandType command = (CommandType)Enum.Parse(typeof(CommandType), messageD["CommandType"]);

            switch (command)
            {
                case CommandType.Login:
                    {
                        LoginEvent?.Invoke(this, new LoginEventArgs()
                        {
                            UserID = messageD["UserID"],
                            PassWord = messageD["PassWord"],
                            ReceiveSocket = clientSocket
                        });

                        break;
                    }
                case CommandType.Logout:
                    {
                        LogoutEvent?.Invoke(this, new User(messageD["UserID"], messageD["NickName"]));
                        break;
                    }
                case CommandType.SignUp:
                    {
                        SignUpEvent?.Invoke(this, new SignUpEventArgs(clientSocket, messageD["PassWord"], messageD["NickName"])));
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
                case CommandType.UserQuit:
                case CommandType.LoginResult:
                case CommandType.SignUpResult:
                case CommandType.ServerDisconnect:
                case CommandType.Remove:
                    {
                        ShowMessage("收到错误的消息类型！");
                        throw new Exception("收到错误的消息类型！");
                    }

            }
        }


        public bool Send(Socket socket, byte[] message, int length)
        {
            try
            {
                socket.Send(message, length, SocketFlags.None);
                return true;
            }
            catch (Exception e)
            {
                ShowMessage(e.Message + "\n" + e.StackTrace + "\n");
                return false;
            }
        }

        public bool Send(Socket socket, string message)
        {
            ShowMessage("向" + socket.RemoteEndPoint.ToString() + "发送消息：" + message + "\n");
            message += ";\0";
            byte[] bytes = Encoding.Default.GetBytes(message);
            return Send(socket, bytes, bytes.Length);
        }

        public bool SendMessage(Socket socket, MessageD chatMessage)
        {
            if (Send(socket, chatMessage.ToString()))
                return true;
            else
            {
                ShowMessage("发送信息失败");
                return false;
            }
        }

        public bool SendLoginResult(UserSocket userSocket, bool result)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append((char)CommandType.LoginResult);
                stringBuilder.Append(separator);
                stringBuilder.Append(result.ToString());
                stringBuilder.Append(separator);
                stringBuilder.Append(userSocket.UserID);
                stringBuilder.Append(separator);
                stringBuilder.Append(userSocket.NickName);
                Send(userSocket.Socket, stringBuilder.ToString());
                return true;
            }
            catch (Exception e)
            {
                ShowMessage("发送登录结果失败" + e.Message + "\n" + e.StackTrace + "\n");
                return false;
            }
        }

        public bool SendSignUpResult(Socket socket,string userID)
        {
            try
            {
                Send(socket, (char)CommandType.SignUpResult+";"+userID);
                return true;
            }
            catch (Exception e)
            {
                ShowMessage("发送" + userID + "的注册结果失败：" + e.Message + "\n" + e.StackTrace + "\n");
                return false;
            }
        }

        public bool SendUserChange(UserSocket oldUserSocket, User newUser, CommandType commandType)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (commandType != CommandType.UserJoin && commandType != CommandType.UserQuit)
            {
                //throw new Exception("发送用户变化错误，非法的命令类型");
                ShowMessage("发送用户变化错误，非法的命令类型");
                return false;
            }
            stringBuilder.Append((char)commandType);
            stringBuilder.Append(separator);
            stringBuilder.Append(newUser.UserID);
            stringBuilder.Append(separator);
            stringBuilder.Append(newUser.NickName);
            try
            {
                Send(oldUserSocket.Socket, stringBuilder.ToString());
                return true;
            }
            catch (Exception e)
            {
                ShowMessage("发送用户变化错误" + e.Message + "\n" + e.StackTrace + "\n");
                return false;
            }
        }

        public bool SendServerClosingMessage(Socket socket)
        {
            return Send(socket, ((char)CommandType.ServerDisconnect).ToString());
        }

        public void Close()
        {
            ServerClosingEvent?.Invoke(this, new EventArgs());
            listening = false;
            if (serverSocket.Connected)
                serverSocket.Disconnect(false);
            serverSocket.Close();
        }
    }
}
