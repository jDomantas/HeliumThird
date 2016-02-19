using Lidgren.Network;

namespace HeliumThird.Events
{
    class EntityUpdate : Event
    {
        public long UID { get; }
        public double X { get; }
        public double Y { get; }
        public int TargetX { get; }
        public int TargetY { get; }
        
        internal EntityUpdate(long uid, double x, double y, int targetX, int targetY)
        {
            UID = uid;
            X = x;
            Y = y;
            TargetX = targetX;
            TargetY = targetY;
        }

        public EntityUpdate(NetIncomingMessage msg, Player sender) : base(sender)
        {
            UID = msg.ReadInt64();
            X = msg.ReadFloat();
            Y = msg.ReadFloat();
            TargetX = msg.ReadInt32();
            TargetY = msg.ReadInt32();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(UID);
            msg.Write((float)X);
            msg.Write((float)Y);
            msg.Write(TargetX);
            msg.Write(TargetY);
        }
    }
}
