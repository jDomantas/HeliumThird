using Lidgren.Network;

namespace HeliumThird.Events
{
    public class EntityRemoval : Event
    {
        public long UID { get; }

        internal EntityRemoval(long uid)
        {
            UID = uid;
        }

        public EntityRemoval(NetIncomingMessage msg, Player sender) : base(sender)
        {
            UID = msg.ReadInt64();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(UID);
        }
    }
}
