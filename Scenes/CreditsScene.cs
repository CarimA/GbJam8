using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GBJamGame.Enums;

namespace GBJamGame.Scenes
{
    public class CreditsScene : IScene
    {
        private Input Input => _game.Input;
        private readonly MainGame _game;

        private float _scrollTime;
        private int _yScroll;
        private int _maxScroll;
        private float _stallTime;

        public CreditsScene(MainGame game)
        {
            _game = game;

            _yScroll = 0;
            _maxScroll = -(8 * Data.Credits.Count) + 144;
            _stallTime = 4f;
        }

        public void Initialise()
        {
            
        }

        public void Update(GameTime gameTIme)
        {
            var elapsed = Input.Down(Actions.A)
                ? gameTIme.GetElapsedSeconds() * 8f
                : gameTIme.GetElapsedSeconds();

            if (_stallTime > 0f)
            {
                _stallTime -= elapsed;
            }
            else
            {
                _scrollTime -= elapsed;
                if (_scrollTime <= 0)
                {
                    _scrollTime += 0.15f;
                    _yScroll -= 1;

                    if (_yScroll < _maxScroll)
                        _yScroll = _maxScroll;
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = _game.SpriteBatch;

            spriteBatch.Begin();
            for (var i = 0; i < Data.Credits.Count; i++)
            {
                spriteBatch.DrawString(Data.Font, Data.Credits[i], 0, _yScroll + (8 * i), Color.Black);
            }
            spriteBatch.End();
        }

        public void Close()
        {

        }
    }
}
