using Lidgren.Network;

namespace HeliumThird.Events
{
    public class ChangeMap : Event
    {
        public int MapWidth { get; }
        public int MapHeight { get; }

        internal ChangeMap(int mapWidth, int mapHeight)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
        }

        public ChangeMap(NetIncomingMessage msg, Player sender) : base(sender)
        {
            MapWidth = msg.ReadInt32();
            MapHeight = msg.ReadInt32();
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            msg.Write(MapWidth);
            msg.Write(MapHeight);
        }
    }
}
