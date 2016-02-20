using Lidgren.Network;

namespace HeliumThird.Events
{
    public class EntityUpdate : Event
    {
        public long UID { get; }
        public double X { get; }
        public double Y { get; }
        public int TargetX { get; }
        public int TargetY { get; }
        public double MoveSpeed { get; }
        
        internal EntityUpdate(long uid, double x, double y, int targetX, int targetY, double speed)
        {
            UID = uid;
            X = x;
            Y = y;
            TargetX = targetX;
            TargetY = targetY;
            MoveSpeed = speed;
        }

        public EntityUpdate(NetIncomingMessage msg, Player sender) : base(sender)
        {
            UID = msg.ReadInt64();
            X = msg.ReadFloat();
            Y = msg.ReadFloat();
            TargetX = msg.ReadInt32();
            TargetY = msg.ReadInt32();
            MoveSpeed = msg.ReadFloat();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(UID);
            msg.Write((float)X);
            msg.Write((float)Y);
            msg.Write(TargetX);
            msg.Write(TargetY);
            msg.Write((float)MoveSpeed);
        }
    }
}
