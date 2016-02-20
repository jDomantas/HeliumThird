using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HeliumThird;

namespace HeliumThirdClient
{
    class ClientMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Tile[,] Tiles { get; private set; }
        public Dictionary<long, ClientEntity> Entities { get; }

        public ClientMap()
        {
            Width = 0;
            Height = 0;
            Tiles = new Tile[0, 0];
            Entities = new Dictionary<long, ClientEntity>();
        }

        public void MapChanged(HeliumThird.Events.ChangeMap e)
        {
            Width = e.MapWidth;
            Height = e.MapHeight;

            Tiles = new Tile[Width, Height];

            Entities.Clear();
        }

        public void AddTileData(HeliumThird.Events.MapData e)
        {
            for (int x = 0; x < Map.MapChunkSize; x++)
                for (int y = 0; y < Map.MapChunkSize; y++)
                    Tiles[x + e.TopLeftX, y + e.TopLeftY] = e.TileData[x, y];
        }

        public void UpdateEntityState(HeliumThird.Events.EntityUpdate e)
        {
            if (Entities.ContainsKey(e.UID))
                Entities[e.UID].UpdateState(e);
            else
                Entities.Add(e.UID, new ClientEntity(e));
        }

        public void RemoveEntity(HeliumThird.Events.EntityRemoval e)
        {
            if (Entities.ContainsKey(e.UID))
                Entities.Remove(e.UID);
        }

        public void Update(double dt)
        {
            foreach (var entity in Entities.Values)
                entity.Update(dt);
        }

        public void Draw(SpriteBatch sb, long focusedEntity, int screenWidth, int screenHeight)
        {
            int cameraX = 0;
            int cameraY = 0;
            if (Entities.ContainsKey(focusedEntity))
            {
                cameraX = (int)Math.Round(Entities[focusedEntity].X * 16) - screenWidth / 2;
                cameraY = (int)Math.Round(Entities[focusedEntity].Y * 16) - screenHeight / 2;
            }

            int left = Math.Max(0, cameraX / 16);
            int top = Math.Max(0, cameraY / 16);
            int right = Math.Min(Width, cameraX / 16 + screenWidth / 16 + 1);
            int bottom = Math.Min(Height, cameraY / 16 + screenHeight / 16 + 1);

            for (int x = left; x < right; x++)
                for (int y = top; y < bottom; y++)
                {
                    if (Tiles[x, y].LayerBottom != -1)
                        sb.Draw(GameHelium.SpriteSheet, new Rectangle(x * 16 - cameraX, y * 16 - cameraY, 16, 16),
                            new Rectangle(Tiles[x, y].LayerBottom % 57 * 17, Tiles[x, y].LayerBottom / 57 * 17, 16, 16), Color.White);

                    if (Tiles[x, y].LayerTop != -1)
                        sb.Draw(GameHelium.SpriteSheet, new Rectangle(x * 16 - cameraX, y * 16 - cameraY, 16, 16),
                            new Rectangle(Tiles[x, y].LayerTop % 57 * 17, Tiles[x, y].LayerTop / 57 * 17, 16, 16), Color.White);
                }

            foreach (var entity in Entities.Values)
            {
                sb.Draw(GameHelium.Pixel, new Rectangle((int)Math.Round(entity.X * 16), (int)Math.Round(entity.Y * 16), 16, 16), Color.Red);
            }
        }
    }
}
