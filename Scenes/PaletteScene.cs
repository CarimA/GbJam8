using System;
using System.Collections.Generic;
using System.Linq;
using GBJamGame.Enums;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBJamGame.Scenes
{
    public class PaletteScene : IScene
    {
        private MainGame _game;
        private IScene _last;

        private Menu _menu;

        private List<(string, Color, Color, Color, Color)> _palettes;
        private int _displayAmount;
        private int _page;

        public PaletteScene(MainGame game, IScene last)
        {
            _game = game;
            _last = last;
            _page = 0;
            _displayAmount = 6;

            _palettes = new List<(string, Color, Color, Color, Color)>()
            {
                {
                    (
                        "Nymph",
                        Constants.StandardColor1,
                        Constants.StandardColor2,
                        Constants.StandardColor3,
                        Constants.StandardColor4
                    )
                },
                {
                    (
                        "Gold",
                        new Color(207, 171, 81),
                        new Color(157, 101, 76),
                        new Color(77, 34, 44),
                        new Color(33, 11, 27)
                    )
                },
                {
                    (
                        "Ice Cream",
                        new Color(255, 237, 211),
                        new Color(249, 157, 117),
                        new Color(235, 107, 111),
                        new Color(124, 63, 88))
                },
                {
                    ("Ayy4",
                        new Color(241, 242, 218),
                        new Color(255, 206, 150),
                        new Color(255, 119, 119),
                        new Color(0, 48, 59))
                },
                {
                    ("Wish",
                        new Color(139, 229, 255),
                        new Color(96, 143, 207),
                        new Color(117, 80, 232),
                        new Color(98, 46, 76))
                },
                {
                    ("Lava",
                        new Color(255, 142, 128),
                        new Color(197, 58, 157),
                        new Color(74, 36, 128),
                        new Color(5, 31, 57))
                },
                {
                    ("Cherrymelon",
                        new Color(252, 222, 234),
                        new Color(255, 77, 109),
                        new Color(38, 89, 53),
                        new Color(1, 40, 36))
                },
                {
                    ("Blush",
                        new Color(254, 145, 146),
                        new Color(252, 222, 190),
                        new Color(12, 192, 212),
                        new Color(94, 87, 104))
                },
                {
                    ("Blueberry",
                        new Color(170, 242, 255),
                        new Color(61, 137, 151),
                        new Color(41, 70, 76),
                        new Color(21, 27, 28))
                },
                {
                    ("Moon",
                        new Color(254, 239, 236),
                        new Color(234, 191, 203),
                        new Color(164, 80, 139),
                        new Color(47, 0, 79))
                },
                {
                    ("Fuzzyfour V2",
                        new Color(255, 255, 158),
                        new Color(70, 255, 156),
                        new Color(255, 0, 135),
                        new Color(36, 17, 120))
                },
                {
                    ("Blues",
                        new Color(229, 241, 243),
                        new Color(123, 168, 184),
                        new Color(48, 97, 123),
                        new Color(8, 38, 59))
                },
                {
                    ("Grayscale",
                        new Color(255, 255, 255),
                        new Color(182, 182, 182),
                        new Color(103, 103, 103),
                        new Color(0, 0, 0))
                },
                {
                    ("Grapefruit",
                        new Color(255, 245, 221),
                        new Color(244, 178, 107),
                        new Color(183, 101, 145),
                        new Color(101, 41, 108))
                },
                {
                    ("JB4",
                        new Color(218, 243, 236),
                        new Color(0, 191, 243),
                        new Color(237, 0, 140),
                        new Color(38, 0, 22))
                },
                {
                    ("Crimson",
                        new Color(239, 249, 214),
                        new Color(186, 80, 68),
                        new Color(122, 28, 75),
                        new Color(27, 3, 38))
                },
                {
                    ("Coldfire",
                        new Color(246, 198, 168),
                        new Color(209, 124, 124),
                        new Color(91, 118, 141),
                        new Color(70, 66, 94))
                },
                {
                    ("Cave4",
                        new Color(228, 203, 191),
                        new Color(147, 130, 130),
                        new Color(79, 78, 128),
                        new Color(44, 0, 22))
                }

            };
            _palettes.Sort((a, b) => a.Item1.CompareTo(b.Item1));

            _menu = new Menu("Palettes");
            UpdateMenu();
        }

        public void Close()
        {

        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = _game.SpriteBatch;

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            Utils.DrawBg(gameTime, spriteBatch);
            _menu.Draw(spriteBatch);
            spriteBatch.End();
        }

        public void Initialise()
        {

        }

        private void UpdateMenu()
        {
            _menu.Clear();

            foreach (var palette in _palettes.Skip(_page * _displayAmount).Take(_displayAmount))
            {
                _menu.AddItem(new MenuLabel(palette.Item1, () =>
                {
                    _game.SetPalette(
                        palette.Item2,
                        palette.Item3,
                        palette.Item4,
                        palette.Item5);
                }));
            }

                _menu.AddItem(new MenuLabel("Next", () =>
                {
                    _page++;
                    if (_page > (_palettes.Count / _displayAmount) - 1)
                        _page = 0;
                    UpdateMenu();
                }));
          
        }

        public void Update(GameTime gameTIme)
        {
            var input = _game.Input;

            if (input.Pressed(Actions.B))
            {
                _game.Transition(_last);
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
    }
}
