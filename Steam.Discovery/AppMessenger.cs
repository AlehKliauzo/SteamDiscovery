using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Steam.Discovery.Models;

namespace Steam.Discovery
{
    public static class AppMessenger
    {
        private static void SendMessage(Message message)
        {
            Messenger.Default.Send<Message>(message);
        }

        public static void SendMessage(AppAction action, object data = null)
        {
            SendMessage(new Message(action, data));
        }

        public static void RegisterForMessage(object recipient, Action<Message> action)
        {
            Messenger.Default.Register(recipient, action);
        }
    }
}
