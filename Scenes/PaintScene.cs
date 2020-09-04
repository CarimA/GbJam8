using System;
using System.Collections.Generic;
using System.Linq;
using GBJamGame.Enums;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        private bool _inMenu;
        private int _menuIndex;
        private int _selectedTone;

        private Tool SelectedAIndex;
        private Tool SelectedBIndex;

        private Menu _startMenu;
        private Menu _confirmMenu;

        public PaintScene(MainGame game, Texture2D texture)
        {
            _game = game;

            _canvasWidth = Constants.GbWidth - 16;
            _canvasHeight = Constants.GbHeight;
            _cursorX = _canvasWidth / 2;
            _cursorY = _canvasHeight / 2;

            _canvas = new Canvas(
                game.GraphicsDevice,
                game.SpriteBatch,
                _canvasWidth,
                _canvasHeight);

            SelectedAIndex = Tool.Pencil1px;
            SelectedBIndex = Tool.Undo;
            _menuIndex = 1;
            _selectedTone = 2;

            _canvas.SetFromTexture(texture);
            
            _inMenu = false;

            _startMenu = new Menu("Menu");
            _startMenu.AddItem(new MenuLabel("Save Sketch", () => { }));
            _startMenu.AddItem(new MenuLabel("Load Sketch", () => { }));
            _startMenu.AddItem(new MenuLabel("Change Palette", () => { }));
            _startMenu.AddItem(new MenuLabel("Return to Menu", () => { }));

            _confirmMenu = new Menu("Quit without saving?");
            _confirmMenu.AddItem(new MenuLabel("Yes", () => { }));
            _confirmMenu.AddItem(new MenuLabel("No", () => { }));
        }

        private Input Input => _game.Input;

        private Audio Audio => _game.Audio;

        public void Initialise()
        {
            
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

            if (_inMenu)
            {
                if (Input.Pressed(Actions.DPadUp))
                    MoveMenu(-1);

                if (Input.Pressed(Actions.DPadDown))
                    MoveMenu(1);

                if (Input.Pressed(Actions.A))
                    ChangeMenu(ref SelectedAIndex);

                if (Input.Pressed(Actions.B))
                    ChangeMenu(ref SelectedBIndex);
            }
            else
            {
                if (Input.Pressed(Actions.DPadUp)
                    || Input.Repeat(Actions.DPadUp, 0.35f, 0.2f))
                    MoveCursor(0, -1);

                if (Input.Pressed(Actions.DPadLeft)
                    || Input.Repeat(Actions.DPadLeft, 0.35f, 0.2f))
                    MoveCursor(-1, 0);

                if (Input.Pressed(Actions.DPadRight)
                    || Input.Repeat(Actions.DPadRight, 0.35f, 0.2f))
                    MoveCursor(1, 0);

                if (Input.Pressed(Actions.DPadDown)
                    || Input.Repeat(Actions.DPadDown, 0.35f, 0.2f))
                    MoveCursor(0, 1);

                if (Input.Pressed(Actions.Start))
                    NextColor();

                if (Input.Pressed(Actions.A))
                    PressAction(SelectedAIndex);
                else if (Input.Down(Actions.A))
                    DownAction(SelectedAIndex);

                if (Input.Pressed(Actions.B))
                    PressAction(SelectedBIndex);
                else if (Input.Down(Actions.B))
                    DownAction(SelectedBIndex);
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
            tool = (Tool) _menuIndex;

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
            _inMenu = !_inMenu;

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


            // draw overlay
            if (_inMenu)
            {
                spriteBatch.Draw(Data.UI, new Rectangle(0, 0, 160, 144), new Rectangle(0, 0, 1, 1), Color.White * 0.65f);
            }
            else
            {
                if (_cursorBlinking)
                {
                    if (_canvas.GetPixel(_cursorX, _cursorY) == ColorIndex.Color4)
                        spriteBatch.Draw(Data.UI, new Vector2(_cursorX + 16, _cursorY), new Rectangle(8, 8, 1, 1),
                            Color.White);
                    else
                        spriteBatch.Draw(Data.UI, new Vector2(_cursorX + 16, _cursorY), new Rectangle(0, 0, 1, 1),
                            Color.White);
                }
            }

            DrawBorder(spriteBatch);
            DrawTones(spriteBatch);
            DrawFillModes(spriteBatch);
            DrawIcons(spriteBatch);
            DrawSelectedIndexes(spriteBatch);

            if (_inMenu)
            {
                if (_cursorBlinking)
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 16 * _menuIndex, 16, 16), new Rectangle(0, 0, 1, 1),
                        Color.White * 0.5f);
                else
                    spriteBatch.Draw(Data.UI, new Rectangle(0, 16 * _menuIndex, 16, 16), new Rectangle(8, 8, 1, 1),
                        Color.White * 0.85f);

                DrawMenuLabels(spriteBatch);
            }

            spriteBatch.End();
        }

        public void Close()
        {
            throw new NotImplementedException();
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
            spriteBatch.Draw(Data.UI, new Vector2(0, 8 + 16 * (int) SelectedBIndex), new Rectangle(0, 48, 8, 8),
                Color.White);
            spriteBatch.Draw(Data.UI, new Vector2(8, 8 + 16 * (int) SelectedAIndex), new Rectangle(8, 48, 8, 8),
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

        private void DrawFillMode(SpriteBatch spriteBatch, int sX, int sY)
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
            DrawMenuLabel(spriteBatch, "Bound to A", 8 + 16 * (int) SelectedAIndex);
            DrawMenuLabel(spriteBatch, "Bound to B", 8 + 16 * (int) SelectedBIndex);
        }

        private void DrawMenuLabel(SpriteBatch spriteBatch, string label, int y)
        {
            spriteBatch.Draw(Data.UI, new Rectangle(16, y, label.Length * 8, 8), new Rectangle(0, 0, 8, 8),
                Color.White);
            spriteBatch.DrawString(Data.Font, label, 16, y, Color.White);
        }
    }
}