using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBJamGame.Globals
{
    public static class Utils
    {
        public static void Save(this Texture2D texture, string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            using var stream = File.Create(filename);
            {
                texture.SaveAsPng(stream, texture.Width, texture.Height);
            }
        }

        public static void DrawString(this SpriteBatch spriteBatch, Texture2D fontTexture, string text, int x, int y,
            Color color)
        {
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i] - 1;
                var sX = c % 16 * 8;
                var sY = c / 16 * 8;
                spriteBatch.Draw(fontTexture, new Rectangle(x + 8 * i, y, 8, 8), new Rectangle(sX, sY, 8, 8),
                    color);
            }
        }

        public static float NextFloat(this Random random, float min, float max)
        {
            var diff = max - min;
            var num = (float)random.NextDouble() * diff;
            return min + num;
        }
    }
}