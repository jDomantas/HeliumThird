using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace HeliumThirdClient
{
    class GameHelium : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        string input = "";
        System.Windows.Forms.Form GameWindow;
        Queue<string> log;

        Connections.Connection connection;

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

            graphics.PreferredBackBufferWidth = 780 / 3 * 2;
            graphics.PreferredBackBufferHeight = 573 / 3 * 2;
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
            else if(e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                System.Diagnostics.Debug.WriteLine($"command: {input}");
                DoCommand(input);
                input = "";
            }
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
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            FontRenderer.SetTexture(Content.Load<Texture2D>("font"));
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
                }
            }

            while (log.Count > 20)
                log.Dequeue();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            int y = 3;
            foreach (var entry in log)
            {
                FontRenderer.RenderText(spriteBatch, entry, 2, y / 3 * 2, Color.Black, 2);
                y += 3 * 8 + 3;
            }
            FontRenderer.RenderText(spriteBatch, "> " + input, 2, (27 * 20 + 6) / 3 * 2, Color.Black, 2);

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
