using HeliumThird.Entities;
using System;
using System.Collections.Generic;

namespace HeliumThird
{
    class Map
    {
        public const int MapChunkSize = 64;

        public World World { get; }
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public int WidthInChunks { get; }
        public int HeightInChunks { get; }

        public Tile[,] Tiles { get; }
        private List<Entity>[,] Entities;

        public Map(World world, string name, int width, int height)
        {
            if (width % MapChunkSize != 0) throw new Exception($"map must be divisible in chunks (given width: {width})");
            if (height % MapChunkSize != 0) throw new Exception($"map must be divisible in chunks (given height: {height})");
            if (width <= 0 || height <= 0) throw new Exception($"invalid map size (given: {width} * {height}");

            World = world;
            Width = width;
            Height = height;
            WidthInChunks = Width / MapChunkSize;
            HeightInChunks = Height / MapChunkSize;

            Tiles = new Tile[width, height];
            Entities = new List<Entity>[WidthInChunks, HeightInChunks];
            for (int x = 0; x < WidthInChunks; x++)
                for (int y = 0; y < HeightInChunks; y++)
                    Entities[x, y] = new List<Entity>();
        }

        public void AddEntity(Entity e)
        {
            Entities[e.GetChunkX(), e.GetChunkY()].Add(e);
        }

        public void Update(double dt)
        {
            for (int x = 0; x < WidthInChunks; x++)
                for (int y = 0; y < HeightInChunks; y++)
                    for (int i = Entities[x, y].Count - 1; i >= 0; i--)
                        Entities[x, y][i].Update(dt);
            
            // check for entities that moved out of their chunks
            for (int x = 0; x < WidthInChunks; x++)
                for (int y = 0; y < HeightInChunks; y++)
                    for (int i = Entities[x, y].Count - 1; i >= 0; i--)
                        if (Entities[x, y][i].GetChunkX() != x || 
                            Entities[x, y][i].GetChunkY() != y)
                        {
                            var entity = Entities[x, y][i];
                            Entities[x, y].RemoveAt(i);
                            AddEntity(entity);
                        }
        }
    }
}
