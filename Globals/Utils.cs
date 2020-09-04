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

        public static void DrawBg(GameTime gameTime, SpriteBatch spriteBatch)
        {
            var offset = ((float)gameTime.TotalGameTime.TotalSeconds % 1f) * 8;

            for (var x = 0; x < (Constants.GbWidth / 8) + 1; x++)
            {
                for (var y = 0; y < (Constants.GbHeight / 8) + 1; y++)
                {
                    spriteBatch.Draw(Data.UI, new Vector2((x * 8) - offset, (y * 8) - offset), new Rectangle(16, 0, 8, 8), Color.White);
                }
            }
        }

        public static bool IsEven(this int num)
        {
            return num % 2 == 0;
        }

        public static Texture2D BlankTexture(GraphicsDevice graphicsDevice)
        {
            return new RenderTarget2D(graphicsDevice, Constants.GbWidth, Constants.GbHeight);
        }
    }
}