using System;
using System.Collections.Generic;
using System.Text;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;

namespace GBJamGame.Scenes
{
    public class StartScene : IScene
    {
        private readonly MainGame _game;
        private float _timer;
        private bool _handled;

        public StartScene(MainGame game)
        {
            _game = game;
            _timer = 2.35f;
            _handled = false;
        }

        public void Initialise()
        {
            
        }

        public void Update(GameTime gameTIme)
        {
            _timer -= gameTIme.GetElapsedSeconds();
            if (_timer <= 0f && !_handled)
            {
                _game.Transition(new TitleScene(_game));
                _handled = true;
            }
        }

        public void Draw(GameTime gameTime)
        {
            _game.GraphicsDevice.Clear(Color.Black);

            _game.SpriteBatch.Begin();
            _game.SpriteBatch.DrawString(Data.Font, "Made for GBJAM8!", 16, 120, Color.White);
            _game.SpriteBatch.End();
        }

        public void Close()
        {

        }
    }
}
