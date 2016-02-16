using HeliumThird.Entities;
using System;
using System.Collections.Generic;

namespace HeliumThird
{
    class World
    {
        private Dictionary<string, Map> Maps;
        private Dictionary<long, Entity> Entities;

        private Random Rand;

        public World(IEnumerable<Map> maps)
        {
            Rand = new Random();

            Maps = new Dictionary<string, Map>();
            foreach (var map in maps)
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
