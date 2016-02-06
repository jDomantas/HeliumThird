using System;
using Lidgren.Network;

namespace HeliumThird.Events
{
    internal sealed class PlayerConnected : Event
    {
        internal PlayerConnected(Player player) : base(player)
        {
            
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            throw new Exception("internal event cannot be serialized");
        }
    }
}
