using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using HeliumThird.Events;

namespace HeliumThirdClient.Connections
{
    class NetworkConnection : Connection
    {
        private NetClient Client { get; }
        private Queue<Event> EventQueue { get; }
        private Event[] SendBuffer { get; }
        private bool IsAttemptingToDisconnect;

        public NetworkConnection(string playerName, IPEndPoint connectTo)
        {
            if (connectTo == null) throw new ArgumentNullException(nameof(connectTo));

            SendBuffer = new Event[1];
            EventQueue = new Queue<Event>();

            NetPeerConfiguration config = new NetPeerConfiguration("helium_third_game");
            config.EnableMessageType(NetIncomingMessageType.Data);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);

            Client = new NetClient(config);
            Client.Start();

            IsAttemptingToDisconnect = false;

            NetOutgoingMessage hail = Client.CreateMessage();
            hail.Write(playerName);
            Client.Connect(connectTo, hail);
        }

        public override void LeaveGame()
        {
            IsAttemptingToDisconnect = true;

            if (Client.ConnectionStatus == NetConnectionStatus.Connected)
                Client.Disconnect("disconnecting");

            Client.Shutdown("disconnecting");
        }

        public override Event ReadMessage()
        {
            if (EventQueue.Count == 0)
                return null;
            else
                return EventQueue.Dequeue();
        }

        public override void SendMessage(Event e)
        {
            SendBuffer[0] = e;
            NetOutgoingMessage msg = Client.CreateMessage();
            Serializer.SerializeEvents(msg, SendBuffer);
            Client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }

        public override void Update(double delta)
        {
            NetIncomingMessage msg;
            while ((msg = Client.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        string message = msg.ReadString();
                        if (string.IsNullOrEmpty(message)) message = "unspecified";
                        ConnectionStatusChanged(status, message);
                        break;

                    case NetIncomingMessageType.Data:
                        ReceivedData(msg);
                        break;
                }

                Client.Recycle(msg);
            }
        }

        private void ConnectionStatusChanged(NetConnectionStatus currentStatus, string message)
        {
            System.Diagnostics.Debug.WriteLine($"connection status changed to {currentStatus}");

            switch (currentStatus)
            {
                case NetConnectionStatus.Connected:
                    EventQueue.Enqueue(new StatusUpdate.JoinedGame());
                    break;

                case NetConnectionStatus.Disconnected:
                    EventQueue.Enqueue(new StatusUpdate.LeftGame(message, !IsAttemptingToDisconnect));
                    break;

                default:
                    break;
            }
        }

        private void ReceivedData(NetIncomingMessage message)
        {
#warning Will crash on incorrectly formatted messages
            foreach (var e in Serializer.DeserializeEvents(message))
                EventQueue.Enqueue(e);
        }
    }
}
