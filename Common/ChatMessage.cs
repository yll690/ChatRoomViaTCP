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

        public MessageD()
        {

        }

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

}
