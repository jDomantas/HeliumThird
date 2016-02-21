using System;

namespace HeliumThird.Entities
{
    class Entity
    {
        public long UID { get; }
        public Map Map { get; private set; }
        public double X { get; protected set; }
        public double Y { get; protected set; }
        public bool Removed { get; private set; }

        public bool IsMoving { get; private set; }
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
            if (!IsMoving)
                return;

            double dx = MovingToX - X;
            double dy = MovingToY - Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double dist = Math.Max(0, MovingSpeed * dt);

            if (len <= dist)
            {
                X = MovingToX;
                Y = MovingToY;
                IsMoving = false;
            }
            else
            {
                X += dx / len * dist;
                Y += dy / len * dist;
            }
        }

        protected void MoveTo(int x, int y, double speed)
        {
            IsMoving = true;
            MovingToX = x;
            MovingToY = y;
            MovingSpeed = speed;

            Map.World.NotifyEntityUpdate(this);
        }

        public int GetChunkX()
        {
            int chunkX = (int)Math.Floor(X / Map.MapChunkSize);

            if (chunkX < 0)
                return 0;
            else if (chunkX >= Map.WidthInChunks)
                return Map.WidthInChunks - 1;
            else
                return chunkX;
        }

        public int GetChunkY()
        {
            int chunkY = (int)Math.Floor(Y / Map.MapChunkSize);

            if (chunkY < 0)
                return 0;
            else if (chunkY >= Map.HeightInChunks)
                return Map.HeightInChunks - 1;
            else
                return chunkY;
        }

        public void Remove()
        {
            Removed = true;
        }

        public void TestMove(Direction dir)
        {
            if (MovingSpeed != 0) return;

            int tx = MovingToX;
            int ty = MovingToY;
            switch (dir)
            {
                case Direction.Down: ty++; break;
                case Direction.Up: ty--; break;
                case Direction.Left: tx--; break;
                case Direction.Right: tx++; break;
            }

            if (tx < 0 || ty < 0 || tx >= Map.Width || ty >= Map.Height || Map.Tiles[tx, ty].Type != Tile.ModelType.Ground)
                return;

            MoveTo(tx, ty, 2);
        }

        public Events.Event CreateUpdate()
        {
            return new Events.EntityUpdate(UID, X, Y, MovingToX, MovingToY, MovingSpeed);
        }
    }
}
