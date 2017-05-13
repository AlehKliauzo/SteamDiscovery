using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Discovery
{
    public enum Message
    {
        GamesListChanged, 
        AppClosing,
        DoesntHaveTagsFocused,
        HasTagsFocused,
        TagSympathyChanged
    }
}
