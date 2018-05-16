using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Common
{
    public class Account
    {
        public string UserID { get; private set; }

        public string NickName { get; private set; }

        private string Password { get; set; }

        public Account()
        {

        }

        public Account(string userID, string password, string nickName)
        {
            UserID = userID;
            NickName = nickName;
            Password = password;
        }

        public bool ConfirmPassword(string password)
        {
            if (password.Equals(Password))
                return true;
            else
                return false;
        }

        public static Account Parse(string accountString)
        {
            return Parse(accountString.Split(','));
        }

        public static Account Parse(string[] infos)
        {
            return new Account(infos[0], infos[1], infos[2]);
        }
        public override string ToString()
        {
            return UserID + "," + Password + "," + NickName;
        }
    }
}
