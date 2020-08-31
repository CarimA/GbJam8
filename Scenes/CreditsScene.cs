using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GBJamGame.Scenes
{
    public class CreditsScene : IScene
    {
        private readonly MainGame _game;
        private Texture2D _font;

        private List<string> _credits;
        private float _scrollTime;
        private int _yScroll;
        private int _maxScroll;

        public CreditsScene(MainGame game)
        {
            _game = game;
            _font = Utils.Texture2DFromFile(_game.GraphicsDevice, "assets/font.png");

            _credits = File.ReadAllLines("assets/credits.txt").ToList();
            _yScroll = 0;
            _maxScroll = -(8 * _credits.Count) + 144;
        }

        public void Update(GameTime gameTIme)
        {
            _scrollTime -= gameTIme.GetElapsedSeconds();
            if (_scrollTime <= 0)
            {
                _scrollTime += 0.06f;
                _yScroll -= 1;

                if (_yScroll < _maxScroll)
                    _yScroll = _maxScroll;
            }
        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = _game.SpriteBatch;

            spriteBatch.Begin();
            for (var i = 0; i < _credits.Count; i++)
            {
                spriteBatch.DrawString(_font, _credits[i], 0, _yScroll + (8 * i), Color.Black);
            }
            spriteBatch.End();
        }
    }
}
