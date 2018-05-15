using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    public class MessageManager
    {
        ClientConnector connector = ((App)Application.Current).connector;
        ChatWindow GroupChatWindow;
        
        public MessageManager()
        {
            connector.GroupMessageEvent += Connector_GroupMessageEvent;
            connector.PrivateMessageEvent += Connector_PrivateMessageEvent;
            connector.ServerDisconnectEvent += Connector_ServerDisconnectEvent;
            connector.UserJoinEvent += Connector_UserJoinEvent;
            connector.UserQuitEvent += Connector_UserQuitEvent;
        }

        public void StartChatting()
        {

        }

        private void Connector_UserQuitEvent(object sender, User e)
        {
            throw new NotImplementedException();
        }

        private void Connector_UserJoinEvent(object sender, User e)
        {
            throw new NotImplementedException();
        }

        private void Connector_ServerDisconnectEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Connector_PrivateMessageEvent(object sender, MessageDictionary e)
        {
            throw new NotImplementedException();
        }

        private void Connector_GroupMessageEvent(object sender, MessageDictionary e)
        {
            throw new NotImplementedException();
        }
    }
}
