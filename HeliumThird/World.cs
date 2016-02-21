using System;
using System.Collections.Generic;
using HeliumThird.Entities;

namespace HeliumThird
{
    public enum Direction { Up, Down, Left, Right }

    class World
    {
        private Dictionary<string, Map> Maps;
        private Dictionary<long, Entity> Entities;

        private Game Game;
        private Random Rand;

        public World(Game game)
        {
            Game = game;
            Rand = new Random();

            Maps = new Dictionary<string, Map>();
            Entities = new Dictionary<long, Entity>();
        }

        public Map GetMap(string name)
        {
            return Maps[name];
        }

        public void AddMap(Map map)
        {
            if (Maps.ContainsKey(map.Name))
                throw new Exception($"Map with name \"{map.Name}\" is already added to this world");

            Maps.Add(map.Name, map);
        }

        public void Update(double dt)
        {
            foreach (var map in Maps.Values)
                map.Update(dt);
        }
        
        public void AddEntity(Entity e)
        {
            Entities.Add(e.UID, e);
        }

        public void RemoveEntity(Entity e)
        {
            Entities.Remove(e.UID);
            foreach (var player in Game.Connection.GetPlayers())
                player.RemoveEntity(Game, e);
        }

        public void NotifyEntityUpdate(Entity e)
        {
            foreach (var player in Game.Connection.GetPlayers())
                player.UpdateEntity(Game, e);
        }

        public Entity GetEntityByID(long uid)
        {
            if (!Entities.ContainsKey(uid))
                return null;

            return Entities[uid];
        }

        public long GenerateUID()
        {
            long uid;
            do
            {
                // not perfect, but will do for now
                uid = ((long)Rand.Next() << 32) | (long)Rand.Next();
            }
            while (Entities.ContainsKey(uid));

            return uid;
        }
    }
}
