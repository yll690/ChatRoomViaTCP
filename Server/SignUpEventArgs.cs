using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Server
{
    public class SignUpEventArgs
    {
        public Socket SignUpSocket { get; set; }

        public string NickName { get; set; }

        public string Password { get; set; }

        public SignUpEventArgs(Socket socket,string nickname,string password)
        {
            SignUpSocket = socket;
            NickName = nickname;
            Password = password;
        }


    }
}
