using System.Collections.Generic;
using HeliumThird.Events;

namespace HeliumThird.Connections
{
    public abstract class Connection
    {
        /// <summary>
        /// Updates internal state of the connection
        /// </summary>
        internal abstract void Update();

        /// <summary>
        /// Shutdowns the connection
        /// </summary>
        internal abstract void Shutdown();

        /// <summary>
        /// Returns event send by client, or null if the queue is empty
        /// </summary>
        /// <returns>Oldest event or null</returns>
        internal abstract Event ReadEvent();

        /// <summary>
        /// Sends event to specific player. Player will receive
        /// these events after events sent to everyone.
        /// </summary>
        /// <param name="e">Event to be sent</param>
        /// <param name="recipient">Recipient of the event</param>
        internal abstract void SendToPlayer(Event e, Player recipient);

        /// <summary>
        /// Send event to all players
        /// </summary>
        /// <param name="e"></param>
        internal abstract void SendToAll(Event e);

        /// <summary>
        /// Flushes remaining unsent messages
        /// </summary>
        internal abstract void FlushMessages();

        /// <summary>
        /// Gets all players using this connection
        /// </summary>
        /// <returns></returns>
        internal abstract IEnumerable<Player> GetPlayers();

        /// <summary>
        /// Disconnects specific player from the game
        /// </summary>
        /// <param name="player">Player to be disconnected</param>
        /// <param name="reason">Message to be displayed</param>
        internal abstract void DisconnectPlayer(Player player, string reason);

        /// <summary>
        /// Gets the next player in queue that wants to join the game,
        /// accept or decline with AcceptPlayer() or DeclinePlayer()
        /// </summary>
        /// <returns></returns>
        internal abstract Player GetPendingPlayer();

        /// <summary>
        /// Accept the last player received by GetPendingPlayer()
        /// </summary>
        internal abstract void AcceptPlayer();

        /// <summary>
        /// Decline last player received by GetPendingPlayer()
        /// </summary>
        /// <param name="reason">Message to be displayed</param>
        internal abstract void DeclinePlayer(string reason);
    }
}
