using System;
using System.Collections.Generic;
using GBJamGame.Enums;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBJamGame.Scenes
{
    public class GalleryScene : IScene
    {
        private MainGame _game;

        private SpriteBatch SpriteBatch => _game.SpriteBatch;
        private Input Input => _game.Input;

        private List<Texture2D> _textures;
        private int _index;
        private int _displayStartIndex;
        private IScene _last;

        public GalleryScene(MainGame game, IScene last, List<Texture2D> textures)
        {
            _game = game;
            _textures = textures;
            _index = 0;
            _last = last;
            _displayStartIndex = 0;
        }

        public void Initialise()
        {

        }

        public void Close()
        {

        }

        public void Update(GameTime gameTIme)
        {
            if (Input.Pressed(Actions.DPadLeft) || Input.Pressed(Actions.DPadRight))
            {
                MoveSelection(true, 0);
            }

            if (Input.Pressed(Actions.DPadUp))
            {
                MoveSelection(false, -1);
            }

            if (Input.Pressed(Actions.DPadDown))
            {
                MoveSelection(false, 1);
            }

            if (Input.Pressed(Actions.B))
            {
                _game.Transition(_last);
            }

            if (Input.Pressed(Actions.A))
            {
                _game.Transition(new PaintScene(_game, _textures[_index]));
            }
        }

        private void MoveSelection(bool moveX, int yDir)
        {
            if (moveX)
            {
                if (_index.IsEven())
                    _index++;
                else
                    _index--;
            }

            if (yDir == -1)
            {
                _index -= 2;
            }

            if (yDir == 1)
            {
                _index += 2;
            }

            if (_index < 0)
                _index = _textures.Count - 1;

            if (_index >= _textures.Count)
                _index = 0;

            _displayStartIndex = _index - _index % 2;
        }

        public void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            Utils.DrawBg(gameTime, SpriteBatch);
            DrawSketch(_displayStartIndex, 8, 8);
            DrawSketch(_displayStartIndex + 1, 76, 8);
            DrawSketch(_displayStartIndex + 2, 8, 76);
            DrawSketch(_displayStartIndex + 3, 76, 76);

            SpriteBatch.Draw(Data.UI, new Rectangle(144, 8, 8, 128), new Rectangle(0, 0, 8, 8), Color.White);

            var perc = ((_index / 2) / (((float)_textures.Count / 2) - 1));
            var scrollY = (120f * perc);
            scrollY = scrollY - scrollY % 8;
            SpriteBatch.Draw(Data.UI, new Rectangle(144, (int)(8 + scrollY), 8, 8), new Rectangle(8, 8, 8, 8), Color.White);
            SpriteBatch.End();
        }

        private void DrawSketch(int index, int x, int y)
        {
            if (!(index > -1 && index < _textures.Count))
                return;

            var size = 60;

            if (_index == index)
            {
                SpriteBatch.Draw(Data.UI, new Rectangle(x - 2, y - 2, size + 4, size + 4), new Rectangle(0, 0, 8, 8), Color.White);
            }

            SpriteBatch.Draw(Data.UI, new Rectangle(x, y, size, size), new Rectangle(8, 8, 8, 8), Color.White);

            SpriteBatch.End();
            SpriteBatch.Begin();
            SpriteBatch.Draw(_textures[index], new Rectangle(x, y, size, size), Color.White);
            SpriteBatch.End();
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }
    }
}
