using System;
using System.Collections.Generic;
using System.Text;
using GBJamGame.Enums;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace GBJamGame.Scenes
{
    public class MenuScene : IScene
    {
        private readonly MainGame _game;
        private Menu _menu;

        public MenuScene(MainGame game)
        {
            _game = game;

            _menu = new Menu("Main Menu");
            _menu.AddItem(new MenuLabel("Color In!", OpenGallery));
            _menu.AddItem(new MenuLabel("Blank Sketch", NewSketch));
            _menu.AddItem(new MenuLabel("Edit a Sketch", () => { }));
            _menu.AddItem(new MenuLabel("Credits", OpenCredits));
            _menu.AddItem(new MenuLabel("Change Palette", OpenPalette));
        }

        private void NewSketch()
        {
            _game.Transition(new PaintScene(_game, Utils.BlankTexture(_game.GraphicsDevice)));
        }

        private void OpenGallery()
        {
            _game.Transition(new GalleryScene(_game, this, Data.Art));
        }

        private void OpenCredits()
        {
            _game.Transition(new CreditsScene(_game));
        }

        private void OpenPalette()
        {
            _game.Transition(new PaletteScene(_game, this));
        }

        public void Initialise()
        {

        }

        public void Update(GameTime gameTIme)
        {
            var input = _game.Input;

            if (input.Pressed(Actions.B))
            {
                _game.Transition(new TitleScene(_game));
            }

            if (input.Pressed(Actions.DPadUp))
            {
                _menu.Previous();
            }

            if (input.Pressed(Actions.DPadDown))
            {
                _menu.Next();
            }

            if (input.Pressed(Actions.A))
            {
                _menu.Select();
            }
        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = _game.SpriteBatch;

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            Utils.DrawBg(gameTime, spriteBatch);
            _menu.Draw(spriteBatch);
            spriteBatch.End();
        }

        public void Close()
        {

        }
    }

    public class Menu
    {
        public List<MenuLabel> _labels;
        private string _title;
        private int _menuIndex;
        private int _widest;

        public Menu(string title)
        {
            _title = title;
            _labels = new List<MenuLabel>();
            _widest = _title.Length;
        }

        public void AddItem(MenuLabel menuLabel)
        {
            if (menuLabel.Name.Length > _widest)
                _widest = menuLabel.Name.Length;

            _labels.Add(menuLabel);
        }

        public void Clear()
        {
            _menuIndex = 0;
            _labels.Clear();
            _widest = _title.Length;
        }

        public void Next()
        {
            _menuIndex++;
            if (_menuIndex >= _labels.Count)
                _menuIndex = 0;
        }

        public void Previous()
        {
            _menuIndex--;
            if (_menuIndex < 0)
                _menuIndex = _labels.Count - 1;
        }

        public void Select()
        {
            _labels[_menuIndex].Action();
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            var height = (_labels.Count * 8) + 32;
            var width = (_widest * 8) + 16;
            var x = (Constants.GbWidth / 2) - (width / 2);
            x = x - x % 8;
            var y = (Constants.GbHeight / 2) - (height / 2);
            y = y - y % 8;

            var titleOffset = (width / 2) - ((int)((_title.Length / 2) + 1) * 8) + 8;

            spriteBatch.Draw(Data.UI, new Rectangle(x, y, width, height), new Rectangle(0, 0, 8, 8), Color.White);
            spriteBatch.DrawString(Data.Font, _title, x + titleOffset, y + 8, Color.White);

            for (var i = 0; i < _labels.Count; i++)
            {
                var label = _labels[i];

                if (i == _menuIndex)
                {
                    spriteBatch.Draw(Data.UI, new Rectangle(x + 8, y + 24 + (i * 8), width - 16, 8), new Rectangle(0, 8, 8, 8), Color.White);
                    spriteBatch.DrawString(Data.Font, label.Name, x + 8, y + 24 + (i * 8), Color.Black);
                }
                else
                {
                    spriteBatch.DrawString(Data.Font, label.Name, x + 8, y + 24 + (i * 8), Color.White);
                }
            }

        }


    }

    public class MenuLabel
    {
        public readonly string Name;
        public readonly Action Action;

        public MenuLabel(string name, Action action)
        {
            Name = name;
            Action = action;
        }
    }
}
