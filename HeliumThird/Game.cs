using System.Linq;
using HeliumThird.Connections;

namespace HeliumThird
{
    public class Game
    {
        public enum State { Running, Loading, Closing, Shutdown }

        public State CurrentState { get; private set; }

        internal Connection Connection { get; }
        internal World GameWorld { get; private set; }

        public Game(Connection connection)
        {
            Connection = connection;
            CurrentState = State.Loading;

#warning TODO: start loading game data
            // create small map for testing
            GameWorld = new World(this);
            GameWorld.AddMap(new Map(GameWorld, "Main", Map.MapChunkSize, Map.MapChunkSize));
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
                Connection.SendToPlayer(new Events.ChatMessage($"Players online: {Connection.GetPlayers().Count()}"), e.Sender);

                Entities.Entity playerEntity = new Entities.Entity(GameWorld.GenerateUID(), GameWorld.GetMap("Main"));
                GameWorld.GetMap("Main").AddEntity(playerEntity);
                e.Sender.SetEntity(this, playerEntity);
            }
            else if (e is Events.PlayerDisconnected)
            {
                Connection.SendToAll(new Events.ChatMessage($"{e.Sender.Name} has left"));
                e.Sender.PlayerEntity.Remove();
            }
            else if (e is Events.PlayerInput)
            {
                e.Sender.PlayerEntity?.TestMove((e as Events.PlayerInput).InputDirection);
            }
        }

        private void UpdateGameState(double delta)
        {
            GameWorld.Update(delta);

            foreach (var player in Connection.GetPlayers())
                player.SendMapData(this);
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
