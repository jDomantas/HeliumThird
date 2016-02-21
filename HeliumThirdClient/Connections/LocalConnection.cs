using System;
using System.Collections.Generic;
using HeliumThird.Events;

namespace HeliumThirdClient.Connections
{
    class LocalConnection : Connection
    {
        private HeliumThird.Connections.LocalConnection ServerConnection { get; }
        private HeliumThird.Game ServerGame { get; }
        private HeliumThird.Game.State LastGameState;
        private Queue<Event> ClientEvents;

        public LocalConnection(string playerName, string saveName)
        {
            ServerConnection = new HeliumThird.Connections.LocalConnection(playerName);
            ServerGame = new HeliumThird.Game(ServerConnection);
            LastGameState = HeliumThird.Game.State.Loading;

            ClientEvents = new Queue<Event>();
        }

        public override void LeaveGame()
        {
            ServerGame.Shutdown();
        }

        public override Event ReadMessage()
        {
            if (ClientEvents.Count != 0)
                return ClientEvents.Dequeue();
            else
                return ServerConnection.ReadServerEvent();
        }

        public override void SendMessage(Event e)
        {
            ServerConnection.EnqueueEvent(e);
        }

        public override void Update(double delta)
        {
            if (LastGameState != ServerGame.CurrentState)
            {
                if (LastGameState == HeliumThird.Game.State.Loading)
                {
                    if (ServerGame.CurrentState == HeliumThird.Game.State.Shutdown)
                    {
                        // loading error
                        ClientEvents.Enqueue(new StatusUpdate.LeftGame("Unknown loading error", true));
                    }
                    else if (ServerGame.CurrentState == HeliumThird.Game.State.Running)
                    {
                        // now in game
                        ClientEvents.Enqueue(new StatusUpdate.JoinedGame());
                    }
                    else
                    {
                        throw new Exception($"changed from {LastGameState} to {ServerGame.CurrentState}, this should not happen");
                    }
                }
                else if (LastGameState == HeliumThird.Game.State.Closing)
                {
                    if (ServerGame.CurrentState == HeliumThird.Game.State.Shutdown)
                    {
                        // saved and exited game
                        ClientEvents.Enqueue(new StatusUpdate.LeftGame("Game saved", false));
                    }
                    else
                    {
                        throw new Exception($"changed from {LastGameState} to {ServerGame.CurrentState}, this should not happen");
                    }
                }

                LastGameState = ServerGame.CurrentState;
            }

            ServerGame.Update(delta);
        }

        public override State GetCurrentState()
        {
            switch (ServerGame.CurrentState)
            {
                case HeliumThird.Game.State.Closing: return State.Leaving;
                case HeliumThird.Game.State.Loading: return State.Joining;
                case HeliumThird.Game.State.Running: return State.InGame;
                case HeliumThird.Game.State.Shutdown: return State.Offline;
            }

            throw new Exception($"server is in invalid state: {ServerGame.CurrentState}");
        }

        public override string GetCurrentStatus()
        {
            switch (ServerGame.CurrentState)
            {
                case HeliumThird.Game.State.Loading: return "Loading map";
                case HeliumThird.Game.State.Closing: return "Saving game";
                default: return "";
            }
        }
    }
}
