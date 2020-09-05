using Microsoft.Xna.Framework;

namespace GBJamGame.Globals
{
    // you save time by doing disgusting practices :^)
    public static class Constants
    {
        public static int GbWidth = 160;
        public static int GbHeight = 144;

        public static Color StandardColor1 = new Color(161, 239, 140);
        public static Color StandardColor2 = new Color(63, 172, 149);
        public static Color StandardColor3 = new Color(68, 97, 118);
        public static Color StandardColor4 = new Color(44, 33, 55);

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
            return (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}