using System.Linq;
using HeliumThird.Connections;

namespace HeliumThird
{
    public class Game
    {
        public enum State { Running, Loading, Closing, Shutdown }

        public State CurrentState { get; private set; }

        private Connection Connection { get; }

        public Game(Connection connection)
        {
            Connection = connection;
            CurrentState = State.Loading;

#warning TODO: start loading game data
        }

        /// <summary>
        /// Update game state
        /// </summary>
        /// <param name="delta">Seconds since last update</param>
        public void Update(double delta)
        {
            if (CurrentState == State.Shutdown)
                return;
            else if (CurrentState == State.Running)
            {
                UpdateGameState(delta);
                UpdateConnection();
            }
            else if (CurrentState == State.Loading)
            {
                UpdateLoading();
                if (CurrentState == State.Running)
                {
                    Connection.Initalize();
                    // done loading
                }
                else if (CurrentState == State.Shutdown)
                {
                    // loading error, should close
                }
            }
            else if (CurrentState == State.Closing)
            {
                UpdateClosing();
            }
        }

        /// <summary>
        /// Saves and exits the game
        /// </summary>
        public void Shutdown()
        {
            Connection.Shutdown();
            CurrentState = State.Closing;
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
                    Connection.AcceptPlayer();
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
            {
                Connection.SendToAll(new Events.ChatMessage($"{e.Sender.Name} has joined"));
                Connection.SendToPlayer(new Events.ChatMessage($"Welcome to test chat, players online: {Connection.GetPlayers().Count()}"), e.Sender);
            }
            else if (e is Events.PlayerDisconnected)
                Connection.SendToAll(new Events.ChatMessage($"{e.Sender.Name} has left"));
        }

        private void UpdateGameState(double delta)
        {

        }

        private void UpdateLoading()
        {
            // load immediately for now
            CurrentState = State.Running;
        }

        private void UpdateClosing()
        {
            // close immediately for now
            CurrentState = State.Shutdown;
        }
    }
}
