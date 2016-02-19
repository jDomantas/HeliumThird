using System;

namespace HeliumThird.Entities
{
    class Entity
    {
        public long UID { get; }
        public Map Map { get; private set; }
        public double X { get; protected set; }
        public double Y { get; protected set; }
        public bool Removed { get; protected set; }

        private int MovingToX;
        private int MovingToY;
        private double MovingSpeed;

        public Entity(long uid, Map map)
        {
            UID = uid;
            Map = map;
        }

        public virtual void Update(double dt)
        {
            UpdateMovement(dt);
        }

        protected void UpdateMovement(double dt)
        {
            double dx = MovingToX - X;
            double dy = MovingToY - Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double dist = MovingSpeed * dt;
            if (len <= dist)
            {
                X = MovingToX;
                Y = MovingToY;
            }
            else
            {
                X += dx / len * dist;
                Y += dy / len * dist;
            }
        }

        public int GetChunkX()
        {
            int chunkX = (int)Math.Floor(X / Map.MapChunkSize);
            if (chunkX < 0) chunkX = 0;
            if (chunkX >= Map.WidthInChunks) chunkX = Map.WidthInChunks - 1;
            return chunkX;
        }

        public int GetChunkY()
        {
            int chunkY = (int)Math.Floor(Y / Map.MapChunkSize);
            if (chunkY < 0) chunkY = 0;
            if (chunkY >= Map.HeightInChunks) chunkY = Map.HeightInChunks - 1;
            return chunkY;
        }

        public Events.Event CreateUpdate()
        {
            return new Events.EntityUpdate(UID, X, Y, MovingToX, MovingToY);
        }
    }
}
