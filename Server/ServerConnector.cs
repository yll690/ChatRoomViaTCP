using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Common;

namespace Server
{
    public class ServerConnector
    {
        public bool log = true;
        public int Port { get; }

        public event EventHandler<MessageDictionary> GroupMessageEvent;
        public event EventHandler<MessageDictionary> PrivateMessageEvent;
        public event EventHandler<LoginEventArgs> LoginEvent;
        public event EventHandler<SignUpEventArgs> SignUpEvent;
        public event EventHandler<User> LogoutEvent;
        public event EventHandler<string> LogEvent;
        public event EventHandler<Socket> DisconnectEvent;
        public event EventHandler ServerClosingEvent;

        private int defaultPort = Properties.Settings.Default.defaultPort;
        private static int bufferLength = StaticStuff.BufferLength;
        private byte[] buffer = new byte[bufferLength];
        private bool listening = true;
        private static char separator = StaticStuff.Separator;
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        public ServerConnector()
        {
            try
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, defaultPort));
            }
            catch(Exception e)
            {
                if (((SocketException)e).ErrorCode == 10048)
                    serverSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            }

            Port = ((IPEndPoint)serverSocket.LocalEndPoint).Port;
            serverSocket.Listen(10);
            Thread listenThread = new Thread(ListenFromClient);
            listenThread.Start();
            
        }

        private void ShowMessage(string s)
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
                    if(e.GetType()==typeof(SocketException))
                    {
                        SocketException se = (SocketException)e;
                        if (se.ErrorCode == 10054)
                        {
                            DisconnectEvent?.Invoke(this, receiveSocket);
                            Thread.CurrentThread.Abort();
                        }
                    }
                    ShowMessage(e.Message + "\n" + e.StackTrace + "\n");
                }
            }
            ShowMessage("接收"+ receiveSocket.RemoteEndPoint.ToString()+ "的线程已结束\n");
        }

        private void MessageSorter(byte[] buffer, int start, int length, Socket clientSocket)
        {
            string content = Encoding.Default.GetString(buffer, 0, length);
            MessageDictionary messageD = new MessageDictionary(content);
            ShowMessage("从" + clientSocket.RemoteEndPoint.ToString() + "接收消息：" + content + "\n");
            CommandType command = (CommandType)Enum.Parse(typeof(CommandType), messageD[MesKeyStr.CommandType]);

            switch (command)
            {
                case CommandType.Login:
                    {
                        LoginEvent?.Invoke(this, new LoginEventArgs()
                        {
                            UserID = messageD[MesKeyStr.UserID],
                            PassWord = messageD[MesKeyStr.PassWord],
                            ReceiveSocket = clientSocket
                        });

                        break;
                    }
                case CommandType.Logout:
                    {
                        LogoutEvent?.Invoke(this, new User(messageD[MesKeyStr.UserID], messageD[MesKeyStr.NickName]));
                        break;
                    }
                case CommandType.SignUp:
                    {
                        SignUpEvent?.Invoke(this, new SignUpEventArgs(clientSocket, messageD[MesKeyStr.NickName], messageD[MesKeyStr.PassWord]));
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

        public bool SendMessage(Socket socket, MessageDictionary chatMessage)
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
                MessageDictionary messageD = new MessageDictionary();
                messageD.Add(MesKeyStr.CommandType, CommandType.LoginResult.ToString());
                messageD.Add(MesKeyStr.LoginResult, result.ToString());
                messageD.Add(MesKeyStr.UserID, userSocket.UserID);
                messageD.Add(MesKeyStr.NickName, userSocket.NickName);
                Send(userSocket.Socket, messageD.ToString());
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
                MessageDictionary messageD = new MessageDictionary();
                messageD.Add(MesKeyStr.CommandType, CommandType.SignUpResult.ToString());
                messageD.Add(MesKeyStr.SignUpResult, userID);
                Send(socket, messageD.ToString());
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
            if (commandType != CommandType.UserJoin && commandType != CommandType.UserQuit)
            {
                ShowMessage("发送用户变化错误，非法的命令类型");
                return false;
            }

            MessageDictionary messageD = new MessageDictionary();
            messageD.Add(MesKeyStr.CommandType, commandType.ToString());
            messageD.Add(MesKeyStr.UserID, newUser.UserID);
            messageD.Add(MesKeyStr.NickName, newUser.NickName);
            try
            {
                Send(oldUserSocket.Socket, messageD.ToString());
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
            MessageDictionary messageD = new MessageDictionary();
            messageD.Add(MesKeyStr.CommandType, CommandType.ServerDisconnect.ToString());
            return Send(socket, messageD.ToString());
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
