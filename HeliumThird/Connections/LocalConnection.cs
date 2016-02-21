using System;
using System.Collections.Generic;
using HeliumThird.Events;

namespace HeliumThird.Connections
{
    /// <summary>
    /// Manages connection for local game with one player
    /// </summary>
    public class LocalConnection : Connection
    {
        private bool IsPlayerAccepted;
        private Player Player;
        private Queue<Event> ClientEventQueue;
        private Queue<Event> ServerEventQueue;

        public LocalConnection(string playerName)
        {
            ClientEventQueue = new Queue<Event>();
            ServerEventQueue = new Queue<Event>();

            IsPlayerAccepted = false;
            Player = new Player(playerName);
        }

        internal override void Initalize()
        {
            
        }

        internal override void DisconnectPlayer(Player player, string reason)
        {
            if (Player == player)
            {
                throw new Exception("I don't know what to do");
            }
        }

        internal override IEnumerable<Player> GetPlayers()
        {
            if (IsPlayerAccepted)
                yield return Player;
        }
        
        internal override Event ReadEvent()
        {
            if (ClientEventQueue.Count == 0)
                return null;
            else
                return ClientEventQueue.Dequeue();
        }

        internal override void SendToAll(Event e)
        {
            ServerEventQueue.Enqueue(e);
        }

        internal override void SendToPlayer(Event e, Player recipient)
        {
            SendToAll(e);
        }

        internal override void Shutdown()
        {
            
        }

        internal override void Update()
        {
            
        }

        internal override void FlushMessages()
        {
            
        }

        /// <summary>
        /// Send event to the server
        /// </summary>
        /// <param name="e">Event to be sent</param>
        public void EnqueueEvent(Event e)
        {
            e.Sender = Player;
            ClientEventQueue.Enqueue(e);
        }

        /// <summary>
        /// Returns event sent by server
        /// </summary>
        /// <returns>Event</returns>
        public Event ReadServerEvent()
        {
            if (ServerEventQueue.Count == 0)
                return null;
            else
                return ServerEventQueue.Dequeue();
        }

        internal override Player GetPendingPlayer()
        {
            if (!IsPlayerAccepted)
                return Player;
            else
                return null;
        }

        internal override void AcceptPlayer()
        {
            IsPlayerAccepted = true;
            ClientEventQueue.Enqueue(new PlayerConnected(Player));
        }

        internal override void DeclinePlayer(string reason)
        {
            throw new Exception("I don't know what to do");
        }
    }
}
