﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HeliumThirdClient
{
    class GameHelium : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static Texture2D SpriteSheet, Pixel;

        string input = "";
        System.Windows.Forms.Form GameWindow;
        Queue<string> log;

        Connections.Connection connection;
        ClientMap map;

        RenderTarget2D gameView;

        public GameHelium()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            GameWindow = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(Window.Handle);
            GameWindow.KeyDown += GameWindow_KeyDown;
            GameWindow.KeyPress += GameWindow_KeyPress;

            System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));
            log = new Queue<string>();

            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
        }

        private void GameWindow_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar >= 32 && e.KeyChar < 127)
                input = input + e.KeyChar;
        }

        private void GameWindow_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Back && input.Length > 0)
                input = input.Substring(0, input.Length - 1);
            else if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                System.Diagnostics.Debug.WriteLine($"command: {input}");
                DoCommand(input);
                input = "";
            }
            else if (e.KeyCode == System.Windows.Forms.Keys.Down)
                connection?.SendMessage(new HeliumThird.Events.PlayerInput(HeliumThird.Direction.Down));
            else if (e.KeyCode == System.Windows.Forms.Keys.Left)
                connection?.SendMessage(new HeliumThird.Events.PlayerInput(HeliumThird.Direction.Left));
            else if (e.KeyCode == System.Windows.Forms.Keys.Right)
                connection?.SendMessage(new HeliumThird.Events.PlayerInput(HeliumThird.Direction.Right));
            else if (e.KeyCode == System.Windows.Forms.Keys.Up)
                connection?.SendMessage(new HeliumThird.Events.PlayerInput(HeliumThird.Direction.Up));
        }

        private void DoCommand(string command)
        {
            string[] splits = command.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length == 0) return;

            if (splits[0] == "connect")
            {
                if (connection != null)
                {
                    log.Enqueue("can't connect, already in game");
                    return;
                }

                if (splits.Length != 3)
                {
                    log.Enqueue("usage: connect <name> <ip address>");
                    return;
                }

                string ipstr = splits[2].Trim();
                System.Net.IPAddress address;
                if (!System.Net.IPAddress.TryParse(ipstr, out address))
                {
                    log.Enqueue($"invalid ip address: {ipstr}");
                    return;
                }

                log.Enqueue($"Joining server at {ipstr} as {splits[1]}");

                System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(address, 8945);
                connection = new Connections.NetworkConnection(splits[1], endPoint);
            }
            else if (splits[0] == "local")
            {
                if (connection != null)
                {
                    log.Enqueue("can't create game, already in game");
                    return;
                }

                if (splits.Length != 2)
                {
                    log.Enqueue("usage: local <name>");
                    return;
                }

                log.Enqueue($"Creating local game as {splits[1]}");
                connection = new Connections.LocalConnection(splits[1], "");
            }
            else if (splits[0] == "leave")
            {
                if (connection == null)
                {
                    log.Enqueue("not in game, can't leave");
                    return;
                }

                if (splits.Length != 1)
                {
                    log.Enqueue("usage: leave");
                    return;
                }

                log.Enqueue($"Leaving game");
                connection.LeaveGame();
                //connection = null;
            }
            else
            {
                if (connection == null)
                {
                    log.Enqueue($"unknown command: {splits[0]}");
                    return;
                }

                connection.SendMessage(new HeliumThird.Events.ChatMessage(command));
            }
        }

        protected override void Initialize()
        {
            map = new ClientMap();
            
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            FontRenderer.SetTexture(Content.Load<Texture2D>("font"));
            SpriteSheet = Content.Load<Texture2D>("base");
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new Color[] { Color.White });

            gameView = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (connection != null)
            {
                connection.Update(1.0 / 60.0);

                HeliumThird.Events.Event input;
                while ((input = connection.ReadMessage()) != null)
                {
                    if (input is HeliumThird.Events.ChatMessage)
                        log.Enqueue((input as HeliumThird.Events.ChatMessage).Message);
                    else if (input is Connections.StatusUpdate.JoinedGame)
                        log.Enqueue("joined game");
                    else if (input is Connections.StatusUpdate.LeftGame)
                    {
                        if ((input as Connections.StatusUpdate.LeftGame).IsError)
                            log.Enqueue($"(error) left game: {(input as Connections.StatusUpdate.LeftGame).Reason}");
                        else
                            log.Enqueue($"left game: {(input as Connections.StatusUpdate.LeftGame).Reason}");

                        connection = null;
                        break;
                    }
                    else if (input is HeliumThird.Events.ChangeMap)
                        map.MapChanged(input as HeliumThird.Events.ChangeMap);
                    else if (input is HeliumThird.Events.MapData)
                        map.AddTileData(input as HeliumThird.Events.MapData);
                    else if (input is HeliumThird.Events.EntityUpdate)
                        map.UpdateEntityState(input as HeliumThird.Events.EntityUpdate);
                    else if (input is HeliumThird.Events.EntityRemoval)
                        map.RemoveEntity(input as HeliumThird.Events.EntityRemoval);
                    else if (input is HeliumThird.Events.ControlledEntityChanged)
                        map.SetControlledEntity(input as HeliumThird.Events.ControlledEntityChanged);
                    else
                    {
                        log.Enqueue($"Unhandled event: {input.GetType().Name}");
                    }
                }
            }

            while (log.Count > 15)
                log.Dequeue();

            if (connection?.GetCurrentState() == Connections.Connection.State.InGame)
                map.Update(1.0 / 60.0);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(gameView);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (connection?.GetCurrentState() == Connections.Connection.State.InGame)
            {
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                map.Draw(spriteBatch, gameView.Width, gameView.Height);
                spriteBatch.End();
            }

            GraphicsDevice.SetRenderTarget(null);
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            spriteBatch.Draw(gameView, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);

            int y = graphics.PreferredBackBufferHeight - 18;
            FontRenderer.RenderText(spriteBatch, "> " + input, 2, y, Color.Black, 2);
            foreach (var entry in System.Linq.Enumerable.Reverse(log))
            {
                y -= 18;
                FontRenderer.RenderText(spriteBatch, entry, 2, y, Color.Black, 2);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (connection != null)
                connection.LeaveGame();

            base.OnExiting(sender, args);
        }
    }
}
