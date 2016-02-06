using Lidgren.Network;

namespace HeliumThird.Events
{
    public abstract class Event
    {
        internal Player Sender { get; set; }

        protected Event()
        {

        }
        
        internal Event(Player sender)
        {
            Sender = sender;
        }

        /// <summary>
        /// Write event to network message
        /// </summary>
        /// <param name="msg">Network message</param>
        public abstract void Serialize(NetOutgoingMessage msg);
    }
}
