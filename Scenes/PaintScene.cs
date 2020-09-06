using GBJamGame.Enums;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GBJamGame.Scenes
{
    public class PaintScene : IScene
    {
        private readonly Canvas _canvas;
        private readonly int _canvasHeight;

        private readonly int _canvasWidth;
        private readonly MainGame _game;

        private bool _cursorBlinking;
        private float _cursorBlinkTime;

        private int _lastPaintedX;
        private int _lastPaintedY;

        private int _cursorX;
        private int _cursorY;
        private int _fillMode;

        private bool _drawingRectangle;
        private bool _drawingLine;
        private int _rectStartX;
        private int _rectStartY;

        public enum MenuState
        {
            Game,
            Tools,
            Options,
            Quit
        }

        private MenuState _inMenu;
        private int _menuIndex;
        private int _selectedTone;

        private Tool SelectedAIndex;
        private Tool SelectedBIndex;

        private readonly Menu _startMenu;
        private readonly Menu _confirmMenu;

        // redesign: turn tools into classes inheriting from ITool, having a name/icon, implementing down/pressed/up and misc things
        // then just putting it in a list of tools and rendering that on the menu!

        public PaintScene(MainGame game, Texture2D texture)
        {
            _game = game;

            _canvasWidth = Constants.GbWidth - 16;
            _canvasHeight = Constants.GbHeight;
            _cursorX = _canvasWidth / 2;
            _cursorY = _canvasHeight / 2;

            _canvas = new Canvas(
                game.GraphicsDevice,
                _canvasWidth,
                _canvasHeight);

            SelectedAIndex = Tool.Pencil1px;
            SelectedBIndex = Tool.Undo;
            _menuIndex = 1;
            _selectedTone = 2;

            _canvas.SetFromTexture(texture);

            _inMenu = MenuState.Game;

            _startMenu = new Menu("Menu");

            _startMenu.AddItem(new MenuLabel("Save Sketch", () =>
            {
                Data.ReloadSavedArt(_game.GraphicsDevice);
                _game.Transition(new GalleryScene(_game, this, Data.SavedArt, (textures, index) =>
                {
                    Data.SaveArt(index, _canvas.GetScreenTexture());
                    _game.Transition(this);
                }));
            }));

            _startMenu.AddItem(new MenuLabel("Load Sketch", () =>
            {
                Data.ReloadSavedArt(_game.GraphicsDevice);
                _game.Transition(new GalleryScene(_game, this, Data.SavedArt, (textures, index) =>
                {
                    _game.Transition(new PaintScene(_game, textures[index]));
                }));
            }));

            _startMenu.AddItem(new MenuLabel("Change Palette", () =>
            {
                _game.Transition(new PaletteScene(_game, this));
            }));

            _startMenu.AddItem(new MenuLabel("Return to Tools", () =>
            {
                _inMenu = MenuState.Tools;
            }));

            _startMenu.AddItem(new MenuLabel("Quit", () =>
            {
                _inMenu = MenuState.Quit;
            }));



            _confirmMenu = new Menu("Quit?");

            _confirmMenu.AddItem(new MenuLabel("Quit & Save", () =>
            {
                Data.ReloadSavedArt(_game.GraphicsDevice);
                _game.Transition(new GalleryScene(_game, this, Data.SavedArt, (textures, index) =>
                {
                    Data.SaveArt(index, _canvas.GetScreenTexture());
                    _game.Transition(new MenuScene(_game));
                }));
            }));

            _confirmMenu.AddItem(new MenuLabel("Quit", () =>
            {
                _game.Transition(new MenuScene(_game));
            }));

            _confirmMenu.AddItem(new MenuLabel("Cancel", () =>
            {
                _inMenu = MenuState.Options;
            }));
        }

        private Input Input => _game.Input;

        private Audio Audio => _game.Audio;

        public void Initialise()
        {
            _game.Audio.StartPlaylist();
        }

        public void Update(GameTime gameTime)
        {
            _cursorBlinkTime -= gameTime.GetElapsedSeconds();
            if (_cursorBlinkTime <= 0f)
            {
                _cursorBlinkTime += 0.375f;
                _cursorBlinking = !_cursorBlinking;
            }

            if (Input.Pressed(Actions.Select))
            {
                ToggleMenu();
            }

            switch (_inMenu)
            {
                case MenuState.Game:
                    if (Input.Pressed(Actions.DPadUp)
                        || Input.Repeat(Actions.DPadUp, 0.35f))
                        MoveCursor(0, -1);

                    if (Input.Pressed(Actions.DPadLeft)
                        || Input.Repeat(Actions.DPadLeft, 0.35f))
                        MoveCursor(-1, 0);

                    if (Input.Pressed(Actions.DPadRight)
                        || Input.Repeat(Actions.DPadRight, 0.35f))
                        MoveCursor(1, 0);

                    if (Input.Pressed(Actions.DPadDown)
                        || Input.Repeat(Actions.DPadDown, 0.35f))
                        MoveCursor(0, 1);

                    if (Input.Pressed(Actions.Start))
                        NextColor();

                    if (Input.Pressed(Actions.A))
                        PressAction(SelectedAIndex);
                    else if (Input.Down(Actions.A))
                        DownAction(SelectedAIndex);
                    else if (Input.Up(Actions.A))
                        UpAction(SelectedAIndex);

                    if (Input.Pressed(Actions.B))
                        PressAction(SelectedBIndex);
                    else if (Input.Down(Actions.B))
                        DownAction(SelectedBIndex);
                    else if (Input.Up(Actions.B))
                        UpAction(SelectedBIndex);

                    break;
                case MenuState.Tools:
                    if (Input.Pressed(Actions.DPadUp))
                        MoveMenu(-1);

                    if (Input.Pressed(Actions.DPadDown))
                        MoveMenu(1);

                    if (Input.Pressed(Actions.A))
                        ChangeMenu(ref SelectedAIndex);

                    if (Input.Pressed(Actions.B))
                        ChangeMenu(ref SelectedBIndex);

                    if (Input.Pressed(Actions.Start))
                        _inMenu = MenuState.Options;

                    break;
                case MenuState.Options:
                    if (Input.Pressed(Actions.B))
                    {
                        _inMenu = MenuState.Tools;
                    }

                    if (Input.Pressed(Actions.DPadUp))
                    {
                        _startMenu.Previous();
                    }

                    if (Input.Pressed(Actions.DPadDown))
                    {
                        _startMenu.Next();
                    }

                    if (Input.Pressed(Actions.A))
                    {
                        _startMenu.Select();
                    }

                    break;
                case MenuState.Quit:
                    if (Input.Pressed(Actions.B))
                    {
                        _inMenu = MenuState.Options;
                    }

                    if (Input.Pressed(Actions.DPadUp))
                    {
                        _confirmMenu.Previous();
                    }

                    if (Input.Pressed(Actions.DPadDown))
                    {
                        _confirmMenu.Next();
                    }

                    if (Input.Pressed(Actions.A))
                    {
                        _confirmMenu.Select();
                    }

                    break;
            }
        }

        private void UpAction(Tool tool)
        {
            var index = _selectedTone switch
            {
                0 => ColorIndex.Color1,
                1 => ColorIndex.Color2,
                2 => ColorIndex.Color3,
                3 => ColorIndex.Color4,
                _ => ColorIndex.Color1
            };

            switch (tool)
            {
                case Tool.Rectangle:
                {
                    if (!_drawingRectangle)
                        return;

                    _drawingRectangle = false;
                    _canvas.Snapshot();

                    var xx = Math.Min(_cursorX, _rectStartX);
                    var yy = Math.Min(_cursorY, _rectStartY);
                    var width = Math.Max(_cursorX, _rectStartX) - xx;
                    var height = Math.Max(_cursorY, _rectStartY) - yy;

                    for (var i = xx; i < xx + width + 1; i++)
                    {
                        _canvas.SetPixel(i, yy, index);
                        _canvas.SetPixel(i, yy + height, index);
                    }

                    for (var i = yy; i < yy + height; i++)
                    {
                        _canvas.SetPixel(xx, i, index);
                        _canvas.SetPixel(xx + width, i, index);
                    }

                    break;
                }

                case Tool.Line:
                    if (!_drawingLine)
                        return;

                    _drawingLine = false;
                    _canvas.Snapshot();

                    var x = _rectStartX;
                    var y = _rectStartY;
                    var x2 = _cursorX;
                    var y2 = _cursorY;

                    int w = x2 - x;
                    int h = y2 - y;
                    int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
                    if (w < 0)
                        dx1 = -1;
                    else if (w > 0)
                        dx1 = 1;
                    if (h < 0)
                        dy1 = -1;
                    else if (h > 0)
                        dy1 = 1;
                    if (w < 0)
                        dx2 = -1;
                    else if (w > 0)
                        dx2 = 1;
                    int longest = Math.Abs(w);
                    int shortest = Math.Abs(h);
                    if (!(longest > shortest))
                    {
                        longest = Math.Abs(h);
                        shortest = Math.Abs(w);
                        if (h < 0)
                            dy2 = -1;
                        else if (h > 0)
                            dy2 = 1;
                        dx2 = 0;
                    }

                    int numerator = longest >> 1;
                    for (int i = 0; i <= longest; i++)
                    {
                        _canvas.SetPixel(x, y, index);
                        numerator += shortest;
                        if (!(numerator < longest))
                        {
                            numerator -= longest;
                            x += dx1;
                            y += dy1;
                        }
                        else
                        {
                            x += dx2;
                            y += dy2;
                        }

                    }
                    break;
            }
        }

        private void MoveMenu(int dir)
        {
            _menuIndex += dir;
            Audio.PlayMenu();

            if (_menuIndex < 1)
                _menuIndex = (int)Tool.FillMode;

            if (_menuIndex > (int)Tool.FillMode)
                _menuIndex = 1;
        }

        private void ChangeMenu(ref Tool tool)
        {
            tool = (Tool)_menuIndex;

            /*switch (tool)
            {
                case Tool.Pencil1px:
                    Audio.PlaySfxHigh("assets/sfx/vox/1px.wav");
                    break;
                case Tool.Pencil2px:
                    Audio.PlaySfxHigh("assets/sfx/vox/2px.wav");
                    break;
                case Tool.Pencil3px:
                    Audio.PlaySfxHigh("assets/sfx/vox/3px.wav");
                    break;
                case Tool.Fill:
                    Audio.PlaySfxHigh("assets/sfx/vox/fill.wav");
                    break;
                case Tool.FillMode:
                    Audio.PlaySfxHigh("assets/sfx/vox/fillmode.wav");
                    break;
                case Tool.Line:
                    Audio.PlaySfxHigh("assets/sfx/vox/line.wav");
                    break;
                case Tool.Rectangle:
                    Audio.PlaySfxHigh("assets/sfx/vox/rectangle.wav");
                    break;
                case Tool.Undo:
                    Audio.PlaySfxHigh("assets/sfx/vox/undo.wav");
                    break;
            }*/
        }

        private void ToggleMenu()
        {
            if (_inMenu != MenuState.Game)
                _inMenu = MenuState.Game;
            else
                _inMenu = MenuState.Tools;

            /*if (_inMenu)
                Audio.PlaySfx("assets/sfx/open.wav");
            else
                Audio.PlaySfx("assets/sfx/close.wav");*/
        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = _game.SpriteBatch;

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            // draw canvas

            spriteBatch.Draw(_canvas.GetScreenTexture(), new Vector2(16, 0), Color.White);

            DrawRectangle(spriteBatch, gameTime);
            DrawLine(spriteBatch, gameTime);

            switch (_inMenu)
            {
                case MenuState.Game:
                    if (_cursorBlinking)
                    {
                        if (_canvas.GetPixel(_cursorX, _cursorY) == ColorIndex.Color4)
                            spriteBatch.Draw(Data.UI, new Vector2(_cursorX + 16, _cursorY), new Rectangle(8, 8, 1, 1),
                                Color.White);
                        else
                            spriteBatch.Draw(Data.UI, new Vector2(_cursorX + 16, _cursorY), new Rectangle(0, 0, 1, 1),
                                Color.White);
                    }

                    break;
                case MenuState.Tools:
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 0, 160, 144), new Rectangle(0, 0, 1, 1), Color.White * 0.65f);

                    break;
            }

            DrawBorder(spriteBatch);
            DrawTones(spriteBatch);
            DrawFillModes(spriteBatch);
            DrawIcons(spriteBatch);
            DrawSelectedIndexes(spriteBatch);

            switch (_inMenu)
            {
                case MenuState.Tools:
                {
                    if (_cursorBlinking)
                        spriteBatch.Draw(Data.UI, new Rectangle(0, 16 * _menuIndex, 16, 16), new Rectangle(0, 0, 1, 1),
                            Color.White * 0.5f);
                    else
                        spriteBatch.Draw(Data.UI, new Rectangle(0, 16 * _menuIndex, 16, 16), new Rectangle(8, 8, 1, 1),
                            Color.White * 0.85f);

                    DrawMenuLabels(spriteBatch);
                    break;
                }
                case MenuState.Options:
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 0, 160, 144), new Rectangle(0, 0, 1, 1), Color.White * 0.65f);

                    _startMenu.Draw(spriteBatch);
                    break;
                case MenuState.Quit:
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 0, 160, 144), new Rectangle(0, 0, 1, 1), Color.White * 0.65f);

                    _confirmMenu.Draw(spriteBatch);
                    break;
            }

            spriteBatch.End();
        }

        private void DrawRectangle(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_drawingRectangle)
            {
                var x = Math.Min(_cursorX, _rectStartX);
                var y = Math.Min(_cursorY, _rectStartY);
                var width = Math.Max(_cursorX, _rectStartX) - x;
                var height = Math.Max(_cursorY, _rectStartY) - y;

                var anim = (int)((gameTime.TotalGameTime.TotalSeconds * 5) % 3);
                switch (anim)
                {
                    case 0:
                        DrawNinePatch(spriteBatch, Data.UI, new Rectangle(16, 8, 8, 8),
                            new Rectangle(16 + x, y, width, height), 2);
                        break;
                    case 1:
                        DrawNinePatch(spriteBatch, Data.UI, new Rectangle(24, 8, 8, 8),
                            new Rectangle(16 + x, y, width, height), 2);
                        break;
                    case 2:
                        DrawNinePatch(spriteBatch, Data.UI, new Rectangle(16, 16, 8, 8),
                            new Rectangle(16 + x, y, width, height), 2);
                        break;

                }
                

            }
        }

        private void DrawLine(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!_drawingLine)
                return;

            var anim = (int)((gameTime.TotalGameTime.TotalSeconds * 5) % 4);
            var x = _rectStartX;
            var y = _rectStartY;
            var x2 = _cursorX;
            var y2 = _cursorY;

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0)
                dx1 = -1;
            else if (w > 0)
                dx1 = 1;
            if (h < 0)
                dy1 = -1;
            else if (h > 0)
                dy1 = 1;
            if (w < 0)
                dx2 = -1;
            else if (w > 0)
                dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0)
                    dy2 = -1;
                else if (h > 0)
                    dy2 = 1;
                dx2 = 0;
            }

            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                switch ((anim + i) % 4)
                {
                    case 0:
                    case 1:
                        spriteBatch.Draw(Data.UI, new Vector2(16 + x, y), new Rectangle(0, 0, 1, 1), Color.White);
                        break;
                    case 2:
                    case 3:
                        spriteBatch.Draw(Data.UI, new Vector2(16 + x, y), new Rectangle(8, 8, 1, 1), Color.White);
                        break;

                }
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        private void DrawNinePatch(SpriteBatch spriteBatch, Texture2D texture, Rectangle source, Rectangle dest,
            int cornerSizes)
        {
            var dWidth = dest.Width - (cornerSizes * 2);
            var dHeight = dest.Height - (cornerSizes * 2);
            var sWidth = source.Width - (cornerSizes * 2);
            var sHeight = source.Height - (cornerSizes * 2);

            if (dWidth > 0)
            {
                for (var x = dest.X + cornerSizes; x < dest.Right - cornerSizes; x += sWidth)
                {
                    if (x > dest.Right - cornerSizes)
                        sWidth = (x - (x - dest.Right - cornerSizes));

                    // top
                    spriteBatch.Draw(texture, new Rectangle(x, dest.Y, sWidth, cornerSizes),
                        new Rectangle(source.X + cornerSizes, source.Y, sWidth, cornerSizes), Color.White);
                    // bottom
                    spriteBatch.Draw(texture,
                        new Rectangle(x, dest.Y + dest.Height - cornerSizes + 1, sWidth, cornerSizes),
                        new Rectangle(source.X + cornerSizes, source.Bottom - cornerSizes, sWidth, cornerSizes),
                        Color.White);
                }
            }

            if (dHeight > 0)
            {
                for (var y = dest.Y + cornerSizes; y < dest.Bottom - cornerSizes; y += sHeight)
                {
                    if (y > dest.Bottom - cornerSizes)
                        sWidth = (y - (y - dest.Bottom - cornerSizes));

                    // left
                    spriteBatch.Draw(texture, new Rectangle(dest.X, y, cornerSizes, sHeight),
                        new Rectangle(source.X, source.Y + cornerSizes, cornerSizes, sHeight), Color.White);
                    // right
                    spriteBatch.Draw(texture,
                        new Rectangle(dest.X + dest.Width - cornerSizes + 1, y, cornerSizes, sHeight),
                        new Rectangle(source.Right - cornerSizes, source.Y + cornerSizes, cornerSizes, sHeight),
                        Color.White);
                }
            }

            if (!(dest.Width > cornerSizes * 2 && dest.Height > cornerSizes * 2))
                cornerSizes = 1;

            // top left
            spriteBatch.Draw(texture, new Rectangle(dest.X, dest.Y, cornerSizes, cornerSizes),
                new Rectangle(source.X, source.Y, cornerSizes, cornerSizes), Color.White);
            // top right
            spriteBatch.Draw(texture,
                new Rectangle(dest.X + dest.Width - cornerSizes + 1, dest.Y, cornerSizes, cornerSizes),
                new Rectangle(source.X + source.Width - cornerSizes, source.Y, cornerSizes, cornerSizes),
                Color.White);
            // bottom left
            spriteBatch.Draw(texture,
                new Rectangle(dest.X, dest.Y + dest.Height - cornerSizes + 1, cornerSizes, cornerSizes),
                new Rectangle(source.X, source.Y + dest.Height - cornerSizes, cornerSizes, cornerSizes),
                Color.White);
            // bottom right
            spriteBatch.Draw(texture,
                new Rectangle(dest.X + dest.Width - cornerSizes + 1, dest.Y + dest.Height - cornerSizes + 1, cornerSizes,
                    cornerSizes),
                new Rectangle(source.X + source.Width - cornerSizes, source.Y + dest.Height - cornerSizes,
                    cornerSizes, cornerSizes), Color.White);

        }

        public void Close()
        {

        }

        private void MoveCursor(int dX, int dY)
        {
            _cursorX += dX;
            _cursorY += dY;

            if (_cursorX < 0)
                _cursorX = _canvasWidth - 1;

            if (_cursorY < 0)
                _cursorY = _canvasHeight - 1;

            if (_cursorX >= _canvasWidth)
                _cursorX = 0;

            if (_cursorY >= _canvasHeight)
                _cursorY = 0;
        }

        private void Pencil(int size, ColorIndex index)
        {
            if (_lastPaintedX == _cursorX &&
                _lastPaintedY == _cursorY)
                return;

            _lastPaintedX = _cursorX;
            _lastPaintedY = _cursorY;

            switch (size)
            {
                case 1:
                    {
                        _canvas.SetPixel(_cursorX, _cursorY, index);
                        break;
                    }
                case 2:
                    {
                        _canvas.SetPixel(_cursorX, _cursorY, index);
                        _canvas.SetPixel(_cursorX - 1, _cursorY, index);
                        _canvas.SetPixel(_cursorX + 1, _cursorY, index);
                        _canvas.SetPixel(_cursorX, _cursorY - 1, index);
                        _canvas.SetPixel(_cursorX, _cursorY + 1, index);
                        break;
                    }
                case 3:
                    {
                        _canvas.SetPixel(_cursorX, _cursorY, index);
                        _canvas.SetPixel(_cursorX - 1, _cursorY, index);
                        _canvas.SetPixel(_cursorX + 1, _cursorY, index);
                        _canvas.SetPixel(_cursorX, _cursorY - 1, index);
                        _canvas.SetPixel(_cursorX, _cursorY + 1, index);
                        _canvas.SetPixel(_cursorX - 1, _cursorY - 1, index);
                        _canvas.SetPixel(_cursorX + 1, _cursorY - 1, index);
                        _canvas.SetPixel(_cursorX - 1, _cursorY + 1, index);
                        _canvas.SetPixel(_cursorX + 1, _cursorY + 1, index);
                        break;
                    }
            }

            //Audio.PlayFill();
        }

        private void NextColor()
        {
            _selectedTone++;
            if (_selectedTone >= 4)
                _selectedTone = 0;
        }

        private void DownAction(Tool tool)
        {
            var index = _selectedTone switch
            {
                0 => ColorIndex.Color1,
                1 => ColorIndex.Color2,
                2 => ColorIndex.Color3,
                3 => ColorIndex.Color4,
                _ => ColorIndex.Color1
            };

            switch (tool)
            {
                case Tool.Pencil1px:
                    {
                        Pencil(1, index);
                        break;
                    }
                case Tool.Pencil2px:
                    {
                        Pencil(2, index);
                        break;
                    }
                case Tool.Pencil3px:
                    {
                        Pencil(3, index);
                        break;
                    }
            }
        }


        private void PressAction(Tool tool)
        {
            var index = _selectedTone switch
            {
                0 => ColorIndex.Color1,
                1 => ColorIndex.Color2,
                2 => ColorIndex.Color3,
                3 => ColorIndex.Color4,
                _ => ColorIndex.Color1
            };

            switch (tool)
            {
                case Tool.FillMode:
                    {
                        _fillMode++;
                        if (_fillMode >= 8)
                            _fillMode = 0;
                        break;
                    }
                case Tool.Pencil1px:
                    {
                        _canvas.Snapshot();
                        Pencil(1, index);
                        break;
                    }
                case Tool.Pencil2px:
                    {
                        _canvas.Snapshot();
                        Pencil(2, index);
                        break;
                    }
                case Tool.Pencil3px:
                    {
                        _canvas.Snapshot();
                        Pencil(3, index);
                        break;
                    }
                case Tool.Fill:
                    {
                        _canvas.Snapshot();
                        Fill(index);
                        break;
                    }
                case Tool.Undo:
                    {
                        _canvas.Undo();
                        break;
                    }
                case Tool.Rectangle:
                {
                    _drawingRectangle = true;
                    _rectStartX = _cursorX;
                    _rectStartY = _cursorY;
                    break;
                }
                case Tool.Line:
                {
                    _drawingLine = true;
                    _rectStartX = _cursorX;
                    _rectStartY = _cursorY;
                    break;
                }
            }
        }

        private void Fill(ColorIndex index)
        {
            var handled = new List<(int, int)>();
            var queue = new Queue<(int, int)>();
            var samplingColor = _canvas.GetPixel(_cursorX, _cursorY);

            queue.Enqueue((_cursorX, _cursorY));

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                if (handled.Contains((x, y)))
                    continue;

                var col = _canvas.GetPixel(x, y);
                if (col == samplingColor && col != index && col != ColorIndex.Transparent)
                {
                    _canvas.SetPixel(x, y, index);

                    switch (_fillMode)
                    {
                        case 1:
                            {
                                if (Math.Abs(_cursorX - x) % 4 == 0 && Math.Abs(_cursorY - y) % 4 == 0)
                                    _canvas.SetPixel(x, y, col);

                                break;
                            }
                        case 2:
                            {
                                if (Math.Abs(_cursorX - x) % 4 == 0 && Math.Abs(_cursorY - y) % 4 == 2
                                    || Math.Abs(_cursorX - x - 2) % 4 == 0 && Math.Abs(_cursorY - y - 2) % 4 == 2)
                                    _canvas.SetPixel(x, y, col);

                                break;
                            }
                        case 3:
                            {
                                if (Math.Abs(_cursorX - x) % 2 == 0 && Math.Abs(_cursorY - y) % 2 == 0)
                                    _canvas.SetPixel(x, y, col);

                                break;
                            }
                        case 4:
                            {
                                if (Math.Abs(_cursorX - x) % 2 == 0 && Math.Abs(_cursorY - y) % 2 == 1
                                    || Math.Abs(_cursorX - x - 1) % 2 == 0 && Math.Abs(_cursorY - y - 1) % 2 == 1)
                                    _canvas.SetPixel(x, y, col);

                                break;
                            }
                        case 5:
                            {
                                if (!(Math.Abs(_cursorX - x) % 2 == 0 && Math.Abs(_cursorY - y) % 2 == 0))
                                    _canvas.SetPixel(x, y, col);

                                break;
                            }
                        case 6:
                            {
                                if (!(Math.Abs(_cursorX - x) % 4 == 0 && Math.Abs(_cursorY - y) % 4 == 2
                                      || Math.Abs(_cursorX - x - 2) % 4 == 0 && Math.Abs(_cursorY - y - 2) % 4 == 2))
                                    _canvas.SetPixel(x, y, col);

                                break;
                            }
                        case 7:
                            {
                                if (!(Math.Abs(_cursorX - x) % 4 == 0 && Math.Abs(_cursorY - y) % 4 == 0))
                                    _canvas.SetPixel(x, y, col);

                                break;
                            }
                    }

                    handled.Add((x, y));

                    queue.Enqueue((x - 1, y));
                    queue.Enqueue((x + 1, y));
                    queue.Enqueue((x, y - 1));
                    queue.Enqueue((x, y + 1));
                }
            }
        }

        private void DrawSelectedIndexes(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Data.UI, new Vector2(0, 8 + 16 * (int)SelectedBIndex), new Rectangle(0, 48, 8, 8),
                Color.White);
            spriteBatch.Draw(Data.UI, new Vector2(8, 8 + 16 * (int)SelectedAIndex), new Rectangle(8, 48, 8, 8),
                Color.White);
        }

        private void DrawIcons(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Data.UI, new Vector2(0, 16), new Rectangle(32, 0, 16, 16), Color.White);
            spriteBatch.Draw(Data.UI, new Vector2(0, 32), new Rectangle(48, 16, 16, 16), Color.White);
            spriteBatch.Draw(Data.UI, new Vector2(0, 48), new Rectangle(48, 32, 16, 16), Color.White);
            spriteBatch.Draw(Data.UI, new Vector2(0, 64), new Rectangle(32, 16, 16, 16), Color.White);
            spriteBatch.Draw(Data.UI, new Vector2(0, 80), new Rectangle(32, 32, 16, 16), Color.White);
            spriteBatch.Draw(Data.UI, new Vector2(0, 96), new Rectangle(32, 48, 16, 16), Color.White);
            spriteBatch.Draw(Data.UI, new Vector2(0, 112), new Rectangle(48, 0, 16, 16), Color.White);
        }

        private void DrawTones(SpriteBatch spriteBatch)
        {
            switch (_selectedTone)
            {
                case 0:
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 0, 15, 16), new Rectangle(8, 8, 8, 8), Color.White);
                    break;
                case 1:
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 0, 15, 16), new Rectangle(0, 8, 8, 8), Color.White);
                    break;
                case 2:
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 0, 15, 16), new Rectangle(8, 0, 8, 8), Color.White);
                    break;
                case 3:
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 0, 15, 16), new Rectangle(0, 0, 8, 8), Color.White);
                    break;
                default:
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 0, 15, 16), new Rectangle(0, 0, 8, 8), Color.White);
                    break;
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch)
        {
            // draw ui
            for (var i = 0; i < 9; i++)
            {
                spriteBatch.Draw(Data.UI, new Vector2(0, i * 16),
                    new Rectangle(8, 8, 8, 8), Color.White);
                spriteBatch.Draw(Data.UI, new Vector2(0, 8 + i * 16),
                    new Rectangle(8, 8, 8, 8), Color.White);
                spriteBatch.Draw(Data.UI, new Vector2(8, i * 16),
                    new Rectangle(8, 40, 8, 8), Color.White);
                spriteBatch.Draw(Data.UI, new Vector2(8, 8 + i * 16),
                    new Rectangle(8, 40, 8, 8), Color.White);
            }
        }

        private void DrawFillModes(SpriteBatch spriteBatch)
        {
            switch (_fillMode)
            {
                case 0:
                    DrawFillMode(spriteBatch, 0, 0);
                    break;
                case 1:
                    DrawFillMode(spriteBatch, 0, 16);
                    break;
                case 2:
                    DrawFillMode(spriteBatch, 8, 16);
                    break;
                case 3:
                    DrawFillMode(spriteBatch, 0, 24);
                    break;
                case 4:
                    DrawFillMode(spriteBatch, 8, 24);
                    break;
                case 5:
                    DrawFillMode(spriteBatch, 0, 32);
                    break;
                case 6:
                    DrawFillMode(spriteBatch, 8, 32);
                    break;
                case 7:
                    DrawFillMode(spriteBatch, 0, 40);
                    break;
                default:
                    DrawFillMode(spriteBatch, 0, 0);
                    break;
            }
        }

        private static void DrawFillMode(SpriteBatch spriteBatch, int sX, int sY)
        {
            spriteBatch.Draw(Data.UI, new Rectangle(0, 128, 8, 8), new Rectangle(sX, sY, 8, 8), Color.White);
            spriteBatch.Draw(Data.UI, new Rectangle(8, 128, 8, 8), new Rectangle(sX, sY, 8, 8), Color.White);
            spriteBatch.Draw(Data.UI, new Rectangle(0, 136, 8, 8), new Rectangle(sX, sY, 8, 8), Color.White);
            spriteBatch.Draw(Data.UI, new Rectangle(8, 136, 8, 8), new Rectangle(sX, sY, 8, 8), Color.White);
        }

        private void DrawMenuLabels(SpriteBatch spriteBatch)
        {
            DrawMenuLabel(spriteBatch, "Color Picker", 0);
            DrawMenuLabel(spriteBatch, "1px Pencil", 16);
            DrawMenuLabel(spriteBatch, "2px Pencil", 32);
            DrawMenuLabel(spriteBatch, "3px Pencil", 48);
            DrawMenuLabel(spriteBatch, "Rectangle", 64);
            DrawMenuLabel(spriteBatch, "Line", 80);
            DrawMenuLabel(spriteBatch, "Undo", 96);
            DrawMenuLabel(spriteBatch, "Fill", 112);
            DrawMenuLabel(spriteBatch, "Fill Mode", 128);

            DrawMenuLabel(spriteBatch, "Bound to START", 8);
            DrawMenuLabel(spriteBatch, "Bound to A", 8 + 16 * (int)SelectedAIndex);
            DrawMenuLabel(spriteBatch, "Bound to B", 8 + 16 * (int)SelectedBIndex);
        }

        private static void DrawMenuLabel(SpriteBatch spriteBatch, string label, int y)
        {
            spriteBatch.Draw(Data.UI, new Rectangle(16, y, label.Length * 8, 8), new Rectangle(0, 0, 8, 8),
                Color.White);
            spriteBatch.DrawString(Data.Font, label, 16, y, Color.White);
        }
    }
}