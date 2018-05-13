using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace Client
{
    public enum MessageType
    {
        Text='T',
        Picture='P'
    }

    public enum MessageDirection
    {
        ToServer,
        ToClient
    }

    public class MessageD:Dictionary<string,string>
    {
        public static char separator = StaticStuff.separator;
        public MessageDirection MessageStage=MessageDirection.ToServer;

        public MessageD(string messageString)
        {
            string[] infos = messageString.Split(';');
            foreach (string s in infos)
            {
                int indexOfColon = s.IndexOf(':');
                if (indexOfColon <= 0) throw new Exception();
                Add(s.Substring(0, indexOfColon), s.Substring(indexOfColon + 1));
            }
        }

        public MessageD(string[] infos)
        {
            foreach (string s in infos)
            {
                int indexOfColon = s.IndexOf(':');
                if (indexOfColon <= 0) throw new Exception();
                Add(s.Substring(0, indexOfColon), s.Substring(indexOfColon + 1));
            }
        }

        public static MessageD Parse(string messageString)
        {
            return new MessageD(messageString);
        }

        public static MessageD Parse(string[] infos)
        {
            return new MessageD(infos);
        }

        public override string ToString()
        {
            return ToString(separator);
        }

        public string ToString(char separator)
        {
            string s="";
            foreach(KeyValuePair<string,string> pair in this)
            {
                s += pair.Key + ":" + pair.Value;
                s += ";";
            }
            return s;
        }
    }

    public class ChatMessageSend
    {
        public MessageType MessageType { get; set; }

        public string UserID { get; set; }

        public string Content { get; set; }

        public string FontFamily { get; set; }

        public int FontSize { get; set; }

        public int FontStyle { get; set; }

        public string FontColor { get; set; }

        public static char separator = StaticStuff.separator;

        public ChatMessageSend()
        {

        }

        public ChatMessageSend(MessageType messageType, string content)
        {
            MessageType = messageType;
            Content = content;
        }

        public override string ToString()
        {
            return ToString(separator);
        }

        public string ToString(char separator)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append((char)MessageType);
            stringBuilder.Append(separator);
            stringBuilder.Append(UserID);
            stringBuilder.Append(separator);
            stringBuilder.Append(Content.Replace(separator + "", "/" + (int)separator + "/"));
            stringBuilder.Append(separator);
            stringBuilder.Append(FontFamily);
            stringBuilder.Append(separator);
            stringBuilder.Append(FontSize);
            stringBuilder.Append(separator);
            stringBuilder.Append(FontStyle);
            stringBuilder.Append(separator);
            stringBuilder.Append(FontColor);
            return stringBuilder.ToString();
        }

        public static ChatMessageSend Parse(string messageString)
        {
            return Parse(messageString.Split(separator));
        }

        public static ChatMessageSend Parse(string[] infos)
        {
            ChatMessageSend chatMessage = new ChatMessageSend()
            {
                MessageType = (MessageType)infos[0][0],
                UserID = infos[1],
                Content = infos[2].Replace("/" + (int)separator + "/", separator + ""),
                FontFamily = infos[3],
                FontSize = int.Parse(infos[4]),
                FontStyle = int.Parse(infos[5]),
                FontColor = infos[6],
            };
            return chatMessage;
        }
    }

    public class ChatMessage : ChatMessageSend
    {
        public string NickName { get; set; }

        public string IP { get; set; }

        public string Time { get; set; }

        public ChatMessage()
        {
        }

        public ChatMessage(ChatMessageSend message, string nickName, string ip, string time)
        {
            MessageType = message.MessageType;
            UserID = message.UserID;
            Content = message.Content;
            FontFamily = message.FontFamily;
            FontSize = message.FontSize;
            FontStyle = message.FontStyle;
            FontColor = message.FontColor;
            NickName = nickName;
            IP = ip;
            Time = time;
        }

        public void AddInfo(string nickName, string ip, string time)
        {
            NickName = nickName;
            IP = ip;
            Time = time;
        }

        new public static ChatMessage Parse(string messageString)
        {
            return Parse(messageString.Split(separator));
        }

        new public static ChatMessage Parse(string[] infos)
        {
            ChatMessage chatMessage = new ChatMessage()
            {
                MessageType = (MessageType)infos[0][0],
                UserID = infos[1],
                Content = infos[2].Replace("/" + (int)separator + "/", separator + ""),
                FontFamily = infos[3],
                FontSize = int.Parse(infos[4]),
                FontStyle = int.Parse(infos[5]),
                FontColor = infos[6],
                NickName = infos[7],
                IP = infos[8],
                Time = infos[9]
            };
            return chatMessage;
        }

        new public string ToString(char separator)
        {
            StringBuilder stringBuilder = new StringBuilder(base.ToString());
            stringBuilder.Append(separator);
            stringBuilder.Append(NickName);
            stringBuilder.Append(separator);
            stringBuilder.Append(IP);
            stringBuilder.Append(separator);
            stringBuilder.Append(Time);
            return stringBuilder.ToString();
        }

    }
}
