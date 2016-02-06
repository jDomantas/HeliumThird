using System;
using Lidgren.Network;

namespace HeliumThird.Events
{
    internal sealed class PlayerDisconnected : Event
    {
        internal PlayerDisconnected(Player player) : base(player)
        {

        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            throw new Exception("internal event cannot be serialized");
        }
    }
}
