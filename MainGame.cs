using System;
using GBJamGame.Enums;
using GBJamGame.Globals;
using GBJamGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GBJamGame
{
    public class MainGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly IScene _scene;
        private RenderTarget2D _backbuffer;

        private Effect _effect;
        private int _lastHeight;

        private int _lastWidth;
        private EffectPass _pass;
        private RenderTarget2D _shaderbuffer;

        public MainGame()
        {
            IsMouseVisible = false;
            InactiveSleepTime = TimeSpan.Zero;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;

            _graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
                SynchronizeWithVerticalRetrace = true
            };
            _graphics.ApplyChanges();

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Input = new Input();

            _scene = new CreditsScene(this);
        }

        public SpriteBatch SpriteBatch { get; }

        public Input Input { get; }

        protected override void Initialize()
        {
            base.Initialize();
            Window.Title = "GAMEBOY CRAYON CLUB";


            _graphics.PreferredBackBufferWidth = Constants.GbWidth * 4;
            _graphics.PreferredBackBufferHeight = Constants.GbHeight * 4;
            _graphics.ApplyChanges();

            _effect = Utils.EffectFromFile(GraphicsDevice, "assets/gb.ogl");
            var lut = Utils.Texture2DFromFile(GraphicsDevice, "assets/lut.png");
            _effect.Parameters["LutTexture"].SetValue(lut);
            _effect.Parameters["LutWidth"].SetValue((float) lut.Width);
            _effect.Parameters["LutHeight"].SetValue((float) lut.Height);
            _effect.Parameters["tone1"].SetValue(Constants.Tone1);
            _effect.Parameters["tone2"].SetValue(Constants.Tone2);
            _effect.Parameters["tone3"].SetValue(Constants.Tone3);
            _effect.Parameters["tone4"].SetValue(Constants.Tone4);
            _pass = _effect.CurrentTechnique.Passes[0];

            _backbuffer = new RenderTarget2D(GraphicsDevice, Constants.GbWidth, Constants.GbHeight);
            _shaderbuffer = new RenderTarget2D(GraphicsDevice, Constants.GbWidth, Constants.GbHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Input.Update(gameTime);
            _scene.Update(gameTime);

            if (Input.Pressed(Actions.Fullscreen))
            {
                _graphics.IsFullScreen = !_graphics.IsFullScreen;
                if (_graphics.IsFullScreen)
                {
                    _lastWidth = _graphics.PreferredBackBufferWidth;
                    _lastHeight = _graphics.PreferredBackBufferHeight;
                    _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
                    _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
                }
                else
                {
                    _graphics.PreferredBackBufferWidth = _lastWidth;
                    _graphics.PreferredBackBufferHeight = _lastHeight;
                }

                _graphics.ApplyChanges();
            }

            if (Input.Pressed(Actions.Screenshot)) TakeScreenshot();
        }

        private void TakeScreenshot()
        {
            var buffer = new RenderTarget2D(GraphicsDevice, Constants.GbWidth * 4, Constants.GbHeight * 4);
            GraphicsDevice.SetRenderTarget(buffer);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            SpriteBatch.Draw(_shaderbuffer, new Rectangle(0, 0, Constants.GbWidth * 4, Constants.GbHeight * 4),
                Color.White);
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            buffer.Save($"screenshots/{Guid.NewGuid()}.png");
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(_backbuffer);
            GraphicsDevice.Clear(Color.White);

            _scene.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(_shaderbuffer);
            GraphicsDevice.Clear(Color.White);

            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
            _effect.Parameters["col1"].SetValue(Constants.Color1);
            _effect.Parameters["col2"].SetValue(Constants.Color2);
            _effect.Parameters["col3"].SetValue(Constants.Color3);
            _effect.Parameters["col4"].SetValue(Constants.Color4);
            _pass.Apply();
            SpriteBatch.Draw(_backbuffer, Vector2.Zero, Color.White);
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            SpriteBatch.Draw(_shaderbuffer, ScaledBackbuffer(), Color.White);
            SpriteBatch.End();
        }

        private Rectangle ScaledBackbuffer()
        {
            var displayWidth = Window.ClientBounds.Width;
            var displayHeight = Window.ClientBounds.Height;
            var width = Constants.GbWidth;
            var height = Constants.GbHeight;
            var widthScale = displayWidth / (double) Constants.GbWidth;
            var heightScale = displayHeight / (double) Constants.GbHeight;
            var smallest = (int) Math.Min(widthScale, heightScale);

            width = width * smallest;
            height = height * smallest;

            var x = displayWidth / 2 - width / 2;
            var y = displayHeight / 2 - height / 2;

            return new Rectangle(x, y, width, height);
        }
    }
}