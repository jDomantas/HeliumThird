using Lidgren.Network;

namespace HeliumThird.Events
{
    public sealed class ChatMessage : Event
    {
        public string Message { get; }

        public ChatMessage(string message)
        {
            Message = message;
        }
        
        public ChatMessage(NetIncomingMessage msg, Player sender) : base(sender)
        {
            Message = msg.ReadString();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(Message);
        }
    }
}
