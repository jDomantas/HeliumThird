using System;
using System.Collections.Generic;
using Lidgren.Network;
using HeliumThird.Events;

namespace HeliumThird.Connections
{
    /// <summary>
    /// Manages connections over the internet
    /// </summary>
    public class NetworkConnection : Connection
    {
        private class NetworkPlayer : Player
        {
            public NetConnection Connection { get; }
            public Queue<Event> PendingEvents { get; }

            public NetworkPlayer(string name, NetConnection connection) : base(name)
            {
                Connection = connection;
                PendingEvents = new Queue<Event>();
            }
        }

        private NetServer Server;
        private List<NetworkPlayer> ConnectedPlayers;
        private Queue<NetworkPlayer> PendingPlayers;
        private NetworkPlayer CurrentPendingPlayer;
        private Queue<Event> EventQueue;
        private Queue<Event> SendingQueue;

        public NetworkConnection(int listenPort)
        {
            ConnectedPlayers = new List<NetworkPlayer>();
            PendingPlayers = new Queue<NetworkPlayer>();
            EventQueue = new Queue<Event>();
            SendingQueue = new Queue<Event>();
            CurrentPendingPlayer = null;
            
            NetPeerConfiguration config = new NetPeerConfiguration("helium_third_game");
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.Data);
            config.EnableMessageType(NetIncomingMessageType.StatusChanged);
            config.Port = listenPort;

            Server = new NetServer(config);
        }

        internal override void Initalize()
        {
            Server.Start();
        }

        internal override void AcceptPlayer()
        {
            if (CurrentPendingPlayer == null)
                throw new Exception("no player to accept");

            CurrentPendingPlayer.Connection.Approve();
            CurrentPendingPlayer = null;
        }

        internal override void DeclinePlayer(string reason)
        {
            if (CurrentPendingPlayer == null)
                throw new Exception("no player to decline");

            CurrentPendingPlayer.Connection.Deny(reason);
            CurrentPendingPlayer = null;
        }

        internal override void DisconnectPlayer(Player player, string reason)
        {
            NetworkPlayer p = player as NetworkPlayer;

            p.Connection.Disconnect(reason);
            ConnectedPlayers.Remove(p);
        }

        internal override Player GetPendingPlayer()
        {
            if (CurrentPendingPlayer != null)
                throw new Exception("did not accept or decline last player before requesting next");

            if (PendingPlayers.Count == 0)
                return null;
            else
                return CurrentPendingPlayer = PendingPlayers.Dequeue();
        }

        internal override IEnumerable<Player> GetPlayers()
        {
            return ConnectedPlayers;
        }
        
        internal override Event ReadEvent()
        {
            if (EventQueue.Count == 0)
                return null;
            else
                return EventQueue.Dequeue();
        }

        internal override void SendToAll(Event e)
        {
            SendingQueue.Enqueue(e);
            if (SendingQueue.Count == 255)
                FlushGlobalMessages();
        }

        internal override void SendToPlayer(Event e, Player recipient)
        {
            NetworkPlayer player = recipient as NetworkPlayer;
            player.PendingEvents.Enqueue(e);
        }

        internal override void FlushMessages()
        {
            FlushGlobalMessages();

            foreach (var player in ConnectedPlayers)
            {
                if (player.PendingEvents.Count > 0)
                {
                    NetOutgoingMessage msg = Server.CreateMessage();
                    Serializer.SerializeEvents(msg, player.PendingEvents);
                    Server.SendMessage(msg, player.Connection, NetDeliveryMethod.ReliableOrdered);
                    player.PendingEvents.Clear();
                }
            }
        }

        internal void FlushGlobalMessages()
        {
            if (SendingQueue.Count == 0) return;

            NetOutgoingMessage msg = Server.CreateMessage();
            Serializer.SerializeEvents(msg, SendingQueue);
            Server.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
            SendingQueue.Clear();
        }

        internal override void Shutdown()
        {
            Server.Shutdown("server is shutting down");
        }

        internal override void Update()
        {
            NetIncomingMessage msg;
            while ((msg = Server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.ConnectionApproval:
                        ConnectionRequest(msg.SenderConnection, msg);
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        ConnectionStatusChanged(msg.SenderConnection, status);
                        break;

                    case NetIncomingMessageType.Data:
                        ReceivedData(msg.SenderConnection.Tag as NetworkPlayer, msg);
                        break;
                }

                Server.Recycle(msg);
            }
        }

        private void ConnectionRequest(NetConnection connection, NetIncomingMessage message)
        {
            string playerName = null;
            try
            {
                playerName = message.ReadString();
            }
            catch { }

            if (playerName == null)
                connection.Deny("invalid message");
            else
            {
                NetworkPlayer player = new NetworkPlayer(playerName, connection);
                PendingPlayers.Enqueue(player);
                connection.Tag = player;
            }
        }

        private void ConnectionStatusChanged(NetConnection connection, NetConnectionStatus currentStatus)
        {
            NetworkPlayer player = connection.Tag as NetworkPlayer;

            switch (currentStatus)
            {
                case NetConnectionStatus.Connected:
                    System.Diagnostics.Debug.WriteLine($"player connected: {player.Name}");
                    ConnectedPlayers.Add(player);
                    EventQueue.Enqueue(new PlayerConnected(player));
                    break;

                case NetConnectionStatus.Disconnected:
                    System.Diagnostics.Debug.WriteLine($"player disconnected: {player.Name}");
                    ConnectedPlayers.Remove(player);
                    EventQueue.Enqueue(new PlayerDisconnected(player));
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"unhandled connection status: {currentStatus}");
                    break;
            }
        }

        private void ReceivedData(NetworkPlayer sender, NetIncomingMessage message)
        {
            foreach (var e in Serializer.DeserializeEvents(message, sender))
                EventQueue.Enqueue(e);
        }
    }
}
