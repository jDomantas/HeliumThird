using System;
using Lidgren.Network;
using HeliumThird.Events;

namespace HeliumThirdClient.Connections.StatusUpdate
{
    class LeftGame : Event
    {
        public string Reason { get; }
        public bool IsError { get; }

        public LeftGame(string reason, bool isError) : base()
        {
            Reason = reason;
            IsError = isError;
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            throw new Exception("can't serialize internal event");
        }
    }
}
