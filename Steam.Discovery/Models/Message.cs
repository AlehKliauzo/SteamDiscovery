namespace Steam.Discovery.Models
{
    public class Message
    {
        public Message(AppAction action, object data = null)
        {
            _action = action;
            _data = data;
        }

        private readonly AppAction _action;
        public AppAction Action
        {
            get { return _action; }
        }

        private readonly object _data;
        public object Data
        {
            get { return _data; }
        }
    }
}
