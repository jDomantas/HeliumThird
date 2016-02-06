using HeliumThird.Events;

namespace HeliumThirdClient.Connections
{
    abstract class Connection
    {
        public abstract void Update(double delta);
        public abstract Event ReadMessage();
        public abstract void SendMessage(Event e);
        public abstract void LeaveGame();
    }
}
