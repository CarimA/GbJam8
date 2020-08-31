using System;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBJamGame.Scenes
{
    public class TitleScene : IScene
    {
        private readonly Texture2D _font;
        private readonly MainGame _game;

        private readonly Texture2D _title;

        public TitleScene(MainGame game)
        {
            _game = game;

            _title = Utils.Texture2DFromFile(game.GraphicsDevice, "assets/title.png");
            _font = Utils.Texture2DFromFile(game.GraphicsDevice, "assets/font.png");
        }

        public void Update(GameTime gameTIme)
        {
        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = _game.SpriteBatch;

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            spriteBatch.Draw(_title, new Rectangle(0, 0, Constants.GbWidth, Constants.GbHeight),
                new Rectangle(0, 0, Constants.GbWidth, Constants.GbHeight), Color.White);

            var catXOffset = (int) (3 * Math.Sin(gameTime.TotalGameTime.TotalSeconds / 1.5));
            var catYOffset = (int) (4 * Math.Sin(gameTime.TotalGameTime.TotalSeconds));
            var catRotate = (float) Math.Sin(gameTime.TotalGameTime.TotalSeconds / 2) * 0.5f;
            spriteBatch.Draw(_title, new Rectangle(21 + catXOffset, 117 + catYOffset, 82, 114),
                new Rectangle(160, 0, 82, 114), Color.White, catRotate, new Vector2(41, 57), SpriteEffects.None, 1f);

            var mouseXOffset = (int) (5 * Math.Sin(gameTime.TotalGameTime.TotalSeconds * 1.4));
            var mouseYOffset = (int) (9 * Math.Cos(gameTime.TotalGameTime.TotalSeconds * 1.1));
            var mouseRotate = (float) Math.Sin(gameTime.TotalGameTime.TotalSeconds / 1.2) * 0.85f;

            spriteBatch.Draw(_title, new Rectangle(70 + mouseXOffset, 125 + mouseYOffset, 46, 57),
                new Rectangle(0, 144, 46, 57), Color.White, mouseRotate, new Vector2(23, 28), SpriteEffects.None, 1f);


            spriteBatch.DrawString(_font, "PRESS", 112, 120, Color.White);
            spriteBatch.DrawString(_font, "START", 112, 128, Color.White);

            spriteBatch.End();
        }
    }
}