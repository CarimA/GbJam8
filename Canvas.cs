using GBJamGame.Enums;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GBJamGame
{
    internal class Canvas
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly int _height;
        private readonly RenderTarget2D _result;

        private readonly LinkedList<ColorIndex[]> _snapshots;
        private readonly SpriteBatch _spriteBatch;

        private readonly int _width;
        private bool _isDirty;

        private ColorIndex[] _pixels;

        public Canvas(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, int width, int height)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _width = width;
            _height = height;
            _result = new RenderTarget2D(graphicsDevice, width, height);
            _snapshots = new LinkedList<ColorIndex[]>();
            Clear();
        }

        public void Clear()
        {
            _pixels = new ColorIndex[_width * _height];
            _isDirty = true;
        }

        public void Snapshot()
        {
            var copy = new ColorIndex[_width * _height];
            Array.Copy(_pixels, copy, _pixels.Length);

            _snapshots.AddFirst(copy);
            if (_snapshots.Count > 20)
                _snapshots.RemoveLast();
        }

        public bool Undo()
        {
            if (_snapshots.Count > 0)
            {
                _pixels = _snapshots.First.Value;
                _snapshots.RemoveFirst();
                _isDirty = true;
                return true;
            }

            return false;
        }

        public void SetFromTexture(Texture2D texture)
        {
            var data = new Color[_width * _height];

            texture.GetData(data);

            for (var i = 0; i < data.Length; i++)
                if (data[i] == Constants.ToneColor1)
                    SetPixel(i % _width, i / _width, ColorIndex.Color1);
                else if (data[i] == Constants.ToneColor2)
                    SetPixel(i % _width, i / _width, ColorIndex.Color2);
                else if (data[i] == Constants.ToneColor3)
                    SetPixel(i % _width, i / _width, ColorIndex.Color3);
                else if (data[i] == Constants.ToneColor4)
                    SetPixel(i % _width, i / _width, ColorIndex.Color4);
                else
                    SetPixel(i % _width, i / _width, ColorIndex.Color1);
        }

        public ColorIndex GetPixel(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return ColorIndex.Transparent;

            return _pixels[x + y * _width];
        }

        public bool SetPixel(int x, int y, ColorIndex color)
        {
            if (GetPixel(x, y) == color)
                return false;

            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return false;

            _pixels[x + y * _width] = color;
            _isDirty = true;
            return true;
        }

        public Texture2D GetScreenTexture()
        {
            if (_isDirty)
            {
                var copy = new Color[_width * _height];
                for (var i = 0; i < _pixels.Length; i++)
                    copy[i] = _pixels[i] switch
                    {
                        ColorIndex.Transparent => Color.Transparent,
                        ColorIndex.Color1 => Constants.ToneColor1,
                        ColorIndex.Color2 => Constants.ToneColor2,
                        ColorIndex.Color3 => Constants.ToneColor3,
                        ColorIndex.Color4 => Constants.ToneColor4,
                        _ => Constants.ToneColor1
                    };
                _result.SetData(copy);
            }

            _isDirty = false;
            return _result;
        }
    }
}