using System;
using iLynx.Networking.ClientServer;

namespace iLynx.Chatter.Infrastructure
{
    public class ChatMessage : IClientMessage<int>
    {
        public int Key { get; set; }
        public Guid ClientId { get; set; }
        public byte[] Data { get; set; }
    }
}