using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class LoginEventArgs:EventArgs
    {
        public string UserID { get; set; }

        public string PassWord { get; set; }

        public Socket ReceiveSocket { get; set; }

        public LoginEventArgs()
        {

        }
    }
}
