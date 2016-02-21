using Lidgren.Network;

namespace HeliumThird.Events
{
    public class ControlledEntityChanged : Event
    {
        public long UID { get; }

        internal ControlledEntityChanged(long uid)
        {
            UID = uid;
        }

        public ControlledEntityChanged(NetIncomingMessage msg, Player sender) : base(sender)
        {
            UID = msg.ReadInt64();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(UID);
        }
    }
}
