using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeliumThirdClient.EntityModels
{
    class HumanoidModel : EntityModel
    {
        private int ShirtColor { get; }

        public HumanoidModel(ClientEntity owner, int shirtColor) : base(owner)
        {
            ShirtColor = shirtColor;
        }

        public override void Draw(SpriteBatch sb, int x, int y)
        {
            sb.Draw(Resources.Textures.Characters, new Rectangle(x, y, 16, 16), new Rectangle(17, 0, 16, 16), Color.White);
            sb.Draw(Resources.Textures.Characters, new Rectangle(x, y, 16, 16), new Rectangle(51, 0, 16, 16), Color.White);

            int shirtX = 6 * 17 + 4 * (ShirtColor % 3) * 17;
            int shirtY = 5 * (ShirtColor / 3) * 17;
            sb.Draw(Resources.Textures.Characters, new Rectangle(x, y, 16, 16), new Rectangle(shirtX, shirtY, 16, 16), Color.White);

            sb.Draw(Resources.Textures.Characters, new Rectangle(x, y, 16, 16), new Rectangle(374, 0, 16, 16), Color.White);
        }
    }
}
