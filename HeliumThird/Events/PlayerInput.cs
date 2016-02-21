using Lidgren.Network;

namespace HeliumThird.Events
{
    public class PlayerInput : Event
    {
        public Direction InputDirection { get; }

        public PlayerInput(Direction direction)
        {
            InputDirection = direction;
        }

        public PlayerInput(NetIncomingMessage msg, Player sender) : base(sender)
        {
            InputDirection = (Direction)msg.ReadByte();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write((byte)InputDirection);
        }
    }
}
