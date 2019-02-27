using System;
using AltV.Net;

namespace ls.gangwar
{
    public class Chat
    {
        private readonly Action<string> broadcast;
        
        public Chat()
        {
            if (Alt.Import("chat", "broadcast", out Action<string> importedBroadcast))
            {
                broadcast = importedBroadcast;
            }
        }

        public void Broadcast(string message)
        {
            broadcast?.Invoke(message);
        }
    }
}