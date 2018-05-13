using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace Client
{
    public class User
    {
        public string UserID { get; set; }
        public string NickName { get; set; }
        public static char separator = StaticStuff.separator;

        public User(string userID)
        {
            UserID = userID;
            NickName = " ";
        }

        public User(string userID, string nickName)
        {
            UserID = userID;
            NickName = nickName;
        }

        public static User Parse(string userSting)
        {
            return Parse(userSting.Split(separator));
        }

        public static User Parse(string[] userInfos)
        {
            return new User(userInfos[0], userInfos[1]);
        }

        public override bool Equals(object obj)
        {
            User user = null;
            try
            {
                user = obj as User;
            }
            catch(Exception e)
            {

            }
            if (user == null)
                return false;
            if (user.UserID == UserID)
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return NickName + "(" + UserID + ")";
        }

        public string ToTransmitString(char separator)
        {
            return UserID + separator + NickName;
        }
        
        public string ToTransmitString()
        {
            return ToTransmitString(separator);
        }

    }
}
