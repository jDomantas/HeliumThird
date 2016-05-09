using Lidgren.Network;
using HeliumThird.Entities;

namespace HeliumThird.Events
{
    public class SetHumanoidModel : Event
    {
        public long OwnerID { get; }
        public int ShirtColor { get; }

        internal SetHumanoidModel(Entity owner, int shirtColor)
        {
            OwnerID = owner.UID;
            ShirtColor = shirtColor;
        }
        
        public SetHumanoidModel(NetIncomingMessage msg, Player sender) : base(sender)
        {
            OwnerID = msg.ReadInt64();
            ShirtColor = msg.ReadInt32();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(OwnerID);
            msg.Write(ShirtColor);
        }
    }
}
