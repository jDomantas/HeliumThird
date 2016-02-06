using System.Linq;
using HeliumThird.Connections;

namespace HeliumThird
{
    public class Game
    {
        private Connection Connection { get; }

        public Game(Connection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// Update game state
        /// </summary>
        /// <param name="delta">Seconds since last update</param>
        public void Update(double delta)
        {
            UpdateConnection();
        }

        /// <summary>
        /// Saves and exits the game
        /// </summary>
        public void Shutdown()
        {
            Connection.Shutdown();
        }

        private void UpdateConnection()
        {
            Connection.Update();

            Player pendingPlayer;
            while ((pendingPlayer = Connection.GetPendingPlayer()) != null)
            {
                if (Connection.GetPlayers().Any(p => p.Name == pendingPlayer.Name))
                    Connection.DeclinePlayer("this name is already used");
                else
                {
                    Connection.AcceptPlayer();
                    Connection.SendToPlayer(new Events.ChatMessage("Welcome to test chat, users online: " + Connection.GetPlayers().Count()), pendingPlayer);
                }
            }

            Events.Event e;
            while ((e = Connection.ReadEvent()) != null)
                HandleEvent(e);

            Connection.FlushMessages();
        }

        private void HandleEvent(Events.Event e)
        {
            if (e is Events.ChatMessage)
                Connection.SendToAll(new Events.ChatMessage($"<{e.Sender.Name}> {(e as Events.ChatMessage).Message}"));
            else if (e is Events.PlayerConnected)
                Connection.SendToAll(new Events.ChatMessage($"{e.Sender.Name} has joined"));
            else if (e is Events.PlayerDisconnected)
                Connection.SendToAll(new Events.ChatMessage($"{e.Sender.Name} has left"));
        }
    }
}
