using System;
using Lidgren.Network;
using HeliumThird.Events;

namespace HeliumThirdClient.Connections.StatusUpdate
{
    class LeftGame : Event
    {
        public string Reason { get; }

        public LeftGame(string reason) : base()
        {
            Reason = reason;
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            throw new Exception("can't serialize internal event");
        }
    }
}
