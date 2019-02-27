using System;
using AltV.Net;
using AltV.Net.Native;

namespace ls.gangwar
{
    public class Chat
    {
        private readonly Action<string> broadcast;

        public Chat()
        {
            var successfully = Alt.Import("chat", "broadcast", out broadcast);
            if (!successfully)
            {
                throw InvalidImportException.Create("chat", "broadcast", MValue.Type.FUNCTION);
            }
        }

        public void Broadcast(string message)
        {
            broadcast?.Invoke(message);
        }
    }
}