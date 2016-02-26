using HeliumThird.Entities;

namespace HeliumThird.AI
{
    class PlayerAI : ICreatureAI
    {
        private Creature Owner;

        public void AttackedBy(Entity source)
        {
            
        }

        public void SetOwner(Creature creature)
        {
            Owner = creature;
        }

        public void Update()
        {
            
        }

        public void AddInput(Direction direction)
        {
            if (Owner != null)
                Owner.Move(direction, 3);
        }
    }
}
