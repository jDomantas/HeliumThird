using HeliumThird.Events;

namespace HeliumThirdClient.Connections
{
    abstract class Connection
    {
        public enum State { Offline, Joining, Leaving, InGame }

        public abstract State GetCurrentState();
        public abstract string GetCurrentStatus();

        /// <summary>
        /// Updates the internal connection state
        /// </summary>
        /// <param name="delta">Seconds passed since last update</param>
        public abstract void Update(double delta);

        /// <summary>
        /// Get next event from this connection
        /// </summary>
        /// <returns></returns>
        public abstract Event ReadMessage();

        /// <summary>
        /// Send event to the server
        /// </summary>
        /// <param name="e"></param>
        public abstract void SendMessage(Event e);

        /// <summary>
        /// Leave current game
        /// </summary>
        public abstract void LeaveGame();
    }
}
