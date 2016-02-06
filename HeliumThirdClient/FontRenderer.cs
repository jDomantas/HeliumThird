using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HeliumThirdClient
{
    static class FontRenderer
    {
        private static int[] SymbolWidth = new int[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            4, 1, 3, 6, 5, 5, 6, 1, 2, 2, 5, 5, 1, 5, 1, 3,
            5, 3, 5, 5, 5, 5, 5, 5, 5, 5, 1, 1, 3, 5, 3, 5,
            7, 5, 5, 5, 5, 5, 5, 5, 5, 3, 5, 5, 4, 7, 6, 5,
            5, 6, 5, 5, 5, 5, 5, 7, 5, 5, 5, 2, 3, 2, 3, 5,
            2, 4, 4, 4, 4, 4, 3, 4, 4, 1, 2, 4, 2, 7, 4, 4,
            4, 4, 4, 4, 3, 4, 5, 5, 5, 4, 4, 3, 1, 3, 6, 0
        };

        private static Texture2D Texture;

        public static void SetTexture(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
                data[i] = (data[i] == Color.Black ? Color.White : new Color(0, 0, 0, 0));
            texture.SetData(data);

            Texture = texture;
        }

        public static void RenderText(SpriteBatch sb, string text, int x, int y, Color color, int scale = 2)
        {
            for (int i = 0; i < text.Length; i++)
                x += RenderCharacter(sb, text[i], x, y, color, scale);
        }

        private static int RenderCharacter(SpriteBatch sb, char c, int x, int y, Color color, int scale)
        {
            if (c < 32 || c >= 127) return 0;
            int index = c;
            sb.Draw(Texture, new Rectangle(x, y, scale * 8, scale * 8), new Rectangle((c % 16) * 8, (c / 16) * 8, 8, 8), color);
            return (SymbolWidth[index] + 1) * scale;
        }
    }
}
