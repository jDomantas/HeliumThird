using HeliumThird.Entities;

namespace HeliumThird
{
    public class Player
    {
        public string Name { get; }

        private Entity PlayerEntity;
        private Map CurrentMap;
        private bool[,] HasChunk;

        public Player(string name)
        {
            Name = name;

            PlayerEntity = null;
            CurrentMap = null;
            HasChunk = null;
        }

        internal void SetEntity(Entity e)
        {
            PlayerEntity = e;
            CurrentMap = e.Map;
            HasChunk = new bool[e.GetChunkX(), e.GetChunkY()];
        }

        internal void SendMapData(Game game)
        {
            if (PlayerEntity == null)
                return;

            if (CurrentMap != PlayerEntity.Map)
            {
                CurrentMap = PlayerEntity.Map;
                HasChunk = new bool[CurrentMap.WidthInChunks, CurrentMap.HeightInChunks];
            }

            int centerX = PlayerEntity.GetChunkX();
            int centerY = PlayerEntity.GetChunkY();
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    SendChunk(game, centerX + x, centerY + y);
        }

        private void SendChunk(Game game, int x, int y)
        {
            if (x < 0 || y < 0 || x >= CurrentMap.WidthInChunks || y >= CurrentMap.HeightInChunks || HasChunk[x, y])
                return;

            HasChunk[x, y] = true;
            game.Connection.SendToPlayer(new Events.MapData(CurrentMap, x * Map.MapChunkSize, y * Map.MapChunkSize), this);
        }
    }
}
