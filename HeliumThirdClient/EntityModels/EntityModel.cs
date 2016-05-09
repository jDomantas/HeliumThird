using Microsoft.Xna.Framework.Graphics;

namespace HeliumThirdClient.EntityModels
{
    abstract class EntityModel
    {
        public ClientEntity Owner { get; }

        public EntityModel(ClientEntity owner)
        {
            Owner = owner;
        }

        public abstract void Draw(SpriteBatch sb, int x, int y);
    }
}
