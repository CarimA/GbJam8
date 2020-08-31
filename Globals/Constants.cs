using Microsoft.Xna.Framework;

namespace GBJamGame.Globals
{
    // you save time by doing disgusting practices :^)
    public static class Constants
    {
        public static int GbWidth = 160;
        public static int GbHeight = 144;

        public static Vector4 Color1 = new Vector4(161f / 255f, 239f / 255f, 140f / 255f, 1f);
        public static Vector4 Color2 = new Vector4(63f / 255f, 172f / 255f, 149f / 255f, 1f);
        public static Vector4 Color3 = new Vector4(68f / 255f, 97f / 255f, 118f / 255f, 1f);
        public static Vector4 Color4 = new Vector4(44f / 255f, 33f / 255f, 55f / 255f, 1f);

        public static float Tone1 = 255f / 255f;
        public static float Tone2 = 170f / 255f;
        public static float Tone3 = 85f / 255f;
        public static float Tone4 = 0f / 255f;

        public static Color ToneColor1 = new Color(Tone1, Tone1, Tone1);
        public static Color ToneColor2 = new Color(Tone2, Tone2, Tone2);
        public static Color ToneColor3 = new Color(Tone3, Tone3, Tone3);
        public static Color ToneColor4 = new Color(Tone4, Tone4, Tone4);

        public static float GetElapsedSeconds(this GameTime gameTime)
        {
            return (float) gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}