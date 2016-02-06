using HeliumThird.Events;

namespace HeliumThirdClient.Connections
{
    class LocalConnection : Connection
    {
        private HeliumThird.Connections.LocalConnection ServerConnection { get; }
        private HeliumThird.Game ServerGame { get; }

        public LocalConnection(string playerName, string saveName)
        {
            ServerConnection = new HeliumThird.Connections.LocalConnection(playerName);
            ServerGame = new HeliumThird.Game(ServerConnection);
        }

        public override void LeaveGame()
        {
            ServerGame.Shutdown();
        }

        public override Event ReadMessage()
        {
            return ServerConnection.ReadServerEvent();
        }

        public override void SendMessage(Event e)
        {
            ServerConnection.EnqueueEvent(e);
        }

        public override void Update(double delta)
        {
            ServerGame.Update(delta);
        }
    }
}
