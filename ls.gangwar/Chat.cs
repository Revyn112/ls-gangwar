using System;
using AltV.Net;

namespace ls.gangwar
{
    public class Chat
    {
        private readonly Action<string> broadcast;

        public Chat()
        {
            Alt.Import("chat", "broadcast", out broadcast);
        }

        public void Broadcast(string message)
        {
            broadcast?.Invoke(message);
        }
    }
}