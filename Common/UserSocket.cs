using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Common
{
    public class UserSocket:User
    {
        public Socket Socket { get; private set; }
        
        public UserSocket(string userID,string nickName, Socket socket):base(userID,nickName)
        {
            UserID = userID;
            NickName = nickName;
            Socket = socket;
        }

        public User ToUser()
        {
            return new User(UserID, NickName);
        }

    }
}
