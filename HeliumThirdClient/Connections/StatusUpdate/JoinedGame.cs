using System;
using Lidgren.Network;
using HeliumThird.Events;

namespace HeliumThirdClient.Connections.StatusUpdate
{
    class JoinedGame : Event
    {
        public JoinedGame() : base() { }

        public override void Serialize(NetOutgoingMessage msg)
        {
            throw new Exception("can't serialize internal event");
        }
    }
}
