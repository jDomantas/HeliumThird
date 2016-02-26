using HeliumThird.Entities;

namespace HeliumThird.AI
{
    interface ICreatureAI
    {
        void SetOwner(Creature creature);
        void Update();
        void AttackedBy(Entity source);
    }
}
