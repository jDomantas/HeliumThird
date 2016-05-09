using System;
using System.Collections.Generic;
using HeliumThirdClient.Connections;
using Microsoft.Xna.Framework.Graphics;

namespace HeliumThirdClient
{
    class ClientGame
    {
        private ClientMap Map { get; }
        private Connection Connection { get; }

        public Queue<string> ChatLog { get; }
        public Connection.State CurrentState
        {
            get
            {
                return Connection.GetCurrentState() == Connection.State.Offline && !HasDisconnected 
                    ? Connection.State.Joining 
                    : Connection.GetCurrentState();
            }
        }

        private bool HasDisconnected;

        public ClientGame(Connection connection)
        {
            Map = new ClientMap();
            Connection = connection;
            ChatLog = new Queue<string>();
        }

        // temporary hack, should be changed to something nicer
        public void KeyPress(HeliumThird.Direction dir)
        {
            Connection.SendMessage(new HeliumThird.Events.PlayerInput(dir));
        }

        // temporary hack, should be changed to something nicer
        public void ChatInput(string message)
        {
            Connection.SendMessage(new HeliumThird.Events.ChatMessage(message));
        }

        // temporary hack, should be changed to something nicer
        public void LeaveGame()
        {
            Connection.LeaveGame();
        }

        public void Update(double delta)
        {
            Connection.Update(delta);

            HeliumThird.Events.Event input;
            while ((input = Connection.ReadMessage()) != null)
            {
                if (input is HeliumThird.Events.ChatMessage)
                    ChatLog.Enqueue((input as HeliumThird.Events.ChatMessage).Message);
                else if (input is Connections.StatusUpdate.JoinedGame)
                    ChatLog.Enqueue("joined game");
                else if (input is Connections.StatusUpdate.LeftGame)
                {
                    if ((input as Connections.StatusUpdate.LeftGame).IsError)
                        ChatLog.Enqueue($"(error) left game: {(input as Connections.StatusUpdate.LeftGame).Reason}");
                    else
                        ChatLog.Enqueue($"left game: {(input as Connections.StatusUpdate.LeftGame).Reason}");

                    HasDisconnected = true;
                }
                else if (input is HeliumThird.Events.ChangeMap)
                    Map.MapChanged(input as HeliumThird.Events.ChangeMap);
                else if (input is HeliumThird.Events.MapData)
                    Map.AddTileData(input as HeliumThird.Events.MapData);
                else if (input is HeliumThird.Events.EntityUpdate)
                    Map.UpdateEntityState(input as HeliumThird.Events.EntityUpdate);
                else if (input is HeliumThird.Events.EntityRemoval)
                    Map.RemoveEntity(input as HeliumThird.Events.EntityRemoval);
                else if (input is HeliumThird.Events.ControlledEntityChanged)
                    Map.SetControlledEntity(input as HeliumThird.Events.ControlledEntityChanged);
                else if (input is HeliumThird.Events.SetHumanoidModel)
                    Map.CreateHumanoidModel(input as HeliumThird.Events.SetHumanoidModel);
                else
                {
                    ChatLog.Enqueue($"Unhandled event: {input.GetType().Name}");
                }
            }

            if (Connection.GetCurrentState() == Connection.State.InGame)
                Map.Update(1.0 / 60.0);
        }

        public void Draw(SpriteBatch sb, int screenWidth, int screenHeight)
        {
            Map.Draw(sb, screenWidth, screenHeight);
        }
    }
}
