using HeliumThird.AI;
using System;

namespace HeliumThird.Entities
{
    class Creature : Entity
    {
        private ICreatureAI _ai;
        public ICreatureAI AI
        {
            get { return _ai; }
            set { _ai = value; _ai?.SetOwner(this); }
        }

        public Creature(long uid, Map map) : base(uid, map)
        {
            AI = null;
        }
        
        public bool Move(Direction direction, double speed)
        {
            if (IsMoving)
                return false;

            int targetX = (int)Math.Round(X);
            int targetY = (int)Math.Round(Y);
            if (direction == Direction.Down) Y++;
            else if (direction == Direction.Up) Y--;
            else if (direction == Direction.Left) X--;
            else if (direction == Direction.Right) X++;

            if (!Map.CanPassTile(targetX, targetY, this))
                return false;

            if (Map.GetCreatureInTile(targetX, targetY) != null)
                return false;

            MoveTo(targetX, targetY, speed);
            return true;
        }

        public override bool DoesObstructTile(int x, int y)
        {
            // if currently in this tile
            if (Math.Abs(x - X) < 0.5 && Math.Abs(y - Y) <= 0.5)
                return true;

            // if moving to this tile
            if (IsMoving && MovingToX == x && MovingToY == y)
                return true;

            return false;
        }
    }
}
