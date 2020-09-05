using System;
using GBJamGame.Enums;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBJamGame.Scenes
{
    public class TitleScene : IScene
    {
        private readonly MainGame _game;
        private bool _handled;

        public TitleScene(MainGame game)
        {
            _game = game;
            _handled = false;
        }

        public void Initialise()
        {
            _game.Audio.PlayBgm("title");
        }

        public void Update(GameTime gameTIme)
        {
            if (_game.Input.Pressed(Actions.Start) && !_handled)
            {
                _game.Transition(new MenuScene(_game));
                _handled = true;
            }
        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = _game.SpriteBatch;

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            spriteBatch.Draw(Data.Title, new Rectangle(0, 0, Constants.GbWidth, Constants.GbHeight),
                new Rectangle(0, 0, Constants.GbWidth, Constants.GbHeight), Color.White);

            var catXOffset = (int) (3 * Math.Sin(gameTime.TotalGameTime.TotalSeconds / 1.5));
            var catYOffset = (int) (4 * Math.Sin(gameTime.TotalGameTime.TotalSeconds));
            var catRotate = (float) Math.Sin(gameTime.TotalGameTime.TotalSeconds / 2) * 0.5f;
            spriteBatch.Draw(Data.Title, new Rectangle(21 + catXOffset, 117 + catYOffset, 82, 114),
                new Rectangle(160, 0, 82, 114), Color.White, catRotate, new Vector2(41, 57), SpriteEffects.None, 1f);

            var mouseXOffset = (int) (5 * Math.Sin(gameTime.TotalGameTime.TotalSeconds * 1.4));
            var mouseYOffset = (int) (9 * Math.Cos(gameTime.TotalGameTime.TotalSeconds * 1.1));
            var mouseRotate = (float) Math.Sin(gameTime.TotalGameTime.TotalSeconds / 1.2) * 0.85f;

            spriteBatch.Draw(Data.Title, new Rectangle(70 + mouseXOffset, 125 + mouseYOffset, 46, 57),
                new Rectangle(0, 144, 46, 57), Color.White, mouseRotate, new Vector2(23, 28), SpriteEffects.None, 1f);


            spriteBatch.DrawString(Data.Font, "PRESS", 112, 120, Color.White);
            spriteBatch.DrawString(Data.Font, "START", 112, 128, Color.White);

            spriteBatch.End();
        }

        public void Close()
        {
            _game.Audio.StopBgm();
        }
    }
}