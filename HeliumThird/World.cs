using System.Collections.Generic;

namespace HeliumThird
{
    class World
    {
        public Dictionary<string, Map> Maps;

        public World(IEnumerable<Map> maps)
        {
            Maps = new Dictionary<string, Map>();
            foreach (var map in maps)
                Maps.Add(map.Name, map);
        }

        public void Update(double dt)
        {
            foreach (var map in Maps.Values)
                map.Update(dt);
        }
    }
}
