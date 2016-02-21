using System;
using System.Collections.Generic;
using HeliumThird.Entities;

namespace HeliumThird
{
    public class Player
    {
        public string Name { get; }

        internal Entity PlayerEntity { get; private set; }
        private Map CurrentMap;
        private bool[,] HasChunk;
        private HashSet<long> KnownEntities;

        public Player(string name)
        {
            Name = name;

            PlayerEntity = null;
            CurrentMap = null;
            HasChunk = null;
            KnownEntities = new HashSet<long>();
        }

        /// <summary>
        /// Sets entity that is controlled by the player
        /// </summary>
        /// <param name="e">Player's entity</param>
        internal void SetEntity(Game game, Entity e)
        {
            PlayerEntity = e;
            ResetMap(game);

            if (e != null)
                game.Connection.SendToPlayer(new Events.ControlledEntityChanged(e.UID), this);
        }

        /// <summary>
        /// Sends map data (tiles) to the player if the played does not have them already
        /// </summary>
        /// <param name="game"></param>
        internal void SendMapData(Game game)
        {
            if (PlayerEntity == null)
                return;

            if (CurrentMap != PlayerEntity.Map)
                ResetMap(game);

            int centerX = PlayerEntity.GetChunkX();
            int centerY = PlayerEntity.GetChunkY();
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    SendChunk(game, centerX + x, centerY + y);
                    SendEntities(game, centerX + x, centerY + y);
                }

            foreach (var uid in KnownEntities)
            {
                var entity = game.GameWorld.GetEntityByID(uid);
                if (entity.Map != CurrentMap || 
                    Math.Abs(entity.X - PlayerEntity.X) > Map.MapChunkSize * 2 || 
                    Math.Abs(entity.X - PlayerEntity.X) > Map.MapChunkSize * 2)
                {
                    KnownEntities.Remove(entity.UID);
                    // will do fine with removing one entity/frame for now
                    // (will crash if iteration continues when collection was modified)
                    break;
                }
            }
        }

        /// <summary>
        /// Notifies player that entity's state has changed (creates entity if the player doesn't have it)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="entity"></param>
        internal void UpdateEntity(Game game, Entity entity)
        {
            if (!KnownEntities.Contains(entity.UID))
                return;

            game.Connection.SendToPlayer(entity.CreateUpdate(), this);
        }

        /// <summary>
        /// Notifies player that entity has been removed
        /// </summary>
        /// <param name="game"></param>
        /// <param name="entity"></param>
        internal void RemoveEntity(Game game, Entity entity)
        {
            if (KnownEntities.Contains(entity.UID))
            {
                KnownEntities.Remove(entity.UID);
                game.Connection.SendToPlayer(new Events.EntityRemoval(entity.UID), this);
            }
        }

        private void ResetMap(Game game)
        {
            CurrentMap = PlayerEntity.Map;
            HasChunk = new bool[CurrentMap.WidthInChunks, CurrentMap.HeightInChunks];
            KnownEntities.Clear();

            game.Connection.SendToPlayer(new Events.ChangeMap(CurrentMap.Width, CurrentMap.Height), this);
        }

        private void SendChunk(Game game, int x, int y)
        {
            if (x < 0 || y < 0 || x >= CurrentMap.WidthInChunks || y >= CurrentMap.HeightInChunks || HasChunk[x, y])
                return;

            HasChunk[x, y] = true;
            game.Connection.SendToPlayer(new Events.MapData(CurrentMap, x * Map.MapChunkSize, y * Map.MapChunkSize), this);
        }

        private void SendEntities(Game game, int x, int y)
        {
            if (x < 0 || y < 0 || x >= CurrentMap.WidthInChunks || y >= CurrentMap.HeightInChunks)
                return;

            var entityList = CurrentMap.Entities[x, y];
            for (int i = entityList.Count - 1; i >= 0; i--)
                if (!KnownEntities.Contains(entityList[i].UID))
                {
                    KnownEntities.Add(entityList[i].UID);
                    game.Connection.SendToPlayer(entityList[i].CreateUpdate(), this);
                }
        }
    }
}
