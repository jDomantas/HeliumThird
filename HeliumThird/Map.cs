using System;
using System.Collections.Generic;
using HeliumThird.Entities;

namespace HeliumThird
{
    public class Map
    {
        public const int MapChunkSize = 64;

        internal World World { get; }
        internal string Name { get; }
        internal int Width { get; }
        internal int Height { get; }
        internal int WidthInChunks { get; }
        internal int HeightInChunks { get; }

        internal Tile[,] Tiles { get; }
        internal List<Entity>[,] Entities { get; }

        internal Map(World world, string name, int width, int height)
        {
            if (width % MapChunkSize != 0) throw new Exception($"map must be divisible in chunks (given width: {width})");
            if (height % MapChunkSize != 0) throw new Exception($"map must be divisible in chunks (given height: {height})");
            if (width <= 0 || height <= 0) throw new Exception($"invalid map size (given: {width} * {height})");

            World = world;
            Name = name;
            Width = width;
            Height = height;
            WidthInChunks = Width / MapChunkSize;
            HeightInChunks = Height / MapChunkSize;

            Tiles = new Tile[width, height];
            Entities = new List<Entity>[WidthInChunks, HeightInChunks];
            for (int x = 0; x < WidthInChunks; x++)
                for (int y = 0; y < HeightInChunks; y++)
                    Entities[x, y] = new List<Entity>();

            // fill with grass (and add random tree stumps) for testing
            Random rnd = new Random();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y].LayerBottom = 5;
                    Tiles[x, y].LayerTop = (rnd.Next(50) / 49) * 1137 - 1;
                    if (Tiles[x, y].LayerTop != -1)
                        Tiles[x, y].Type = Tile.ModelType.TransparentWall;
                    else
                        Tiles[x, y].Type = Tile.ModelType.Ground;
                    Tiles[x, y].LayerDecoration = -1;
                }
        }

        internal void AddEntity(Entity e)
        {
            Entities[e.GetChunkX(), e.GetChunkY()].Add(e);
            World.AddEntity(e);
        }

        internal void Update(double dt)
        {
            for (int x = 0; x < WidthInChunks; x++)
                for (int y = 0; y < HeightInChunks; y++)
                    for (int i = Entities[x, y].Count - 1; i >= 0; i--)
                        if (Entities[x, y][i].Map == this && !Entities[x, y][i].Removed)
                            Entities[x, y][i].Update(dt);

            // check for entities that moved out of their chunks
            for (int x = 0; x < WidthInChunks; x++)
                for (int y = 0; y < HeightInChunks; y++)
                    for (int i = Entities[x, y].Count - 1; i >= 0; i--)
                    {
                        if (Entities[x, y][i].Removed || Entities[x, y][i].Map != this)
                        {
                            if (Entities[x, y][i].Removed)
                                World.RemoveEntity(Entities[x, y][i]);

                            Entities[x, y].RemoveAt(i);
                        }
                        else if (Entities[x, y][i].GetChunkX() != x || Entities[x, y][i].GetChunkY() != y)
                        {
                            var entity = Entities[x, y][i];
                            Entities[x, y].RemoveAt(i);
                            Entities[entity.GetChunkX(), entity.GetChunkY()].Add(entity);
                        }
                    }
        }
    }
}
