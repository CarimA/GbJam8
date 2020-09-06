using GBJamGame.Enums;
using GBJamGame.Globals;
using GBJamGame.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GBJamGame
{
    public class MainGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private IScene _scene;
        private RenderTarget2D _backbuffer;

        private int _lastHeight;

        private int _lastWidth;
        private EffectPass _pass;
        private RenderTarget2D _shaderbuffer;

        // 0 = black, 1 = clear
        private float _transition = 1f;
        private bool _transitionDirection;
        private bool _isTransitioning;
        private float _transitionWaitTime;
        private IScene _transitionTarget;

        public Vector4 Color1;
        public Vector4 Color2;
        public Vector4 Color3;
        public Vector4 Color4;
        private bool _sceneClosed;

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
            Audio = new Audio();

        }

        public SpriteBatch SpriteBatch { get; }

        public Input Input { get; }

        public Audio Audio { get; }

        protected override void Initialize()
        {
            base.Initialize();
            Window.Title = "GAMEBOY CRAYON CLUB";

            SetPalette(Constants.StandardColor1,
                Constants.StandardColor2,
                Constants.StandardColor3,
                Constants.StandardColor4);

            _graphics.PreferredBackBufferWidth = Constants.GbWidth * 4;
            _graphics.PreferredBackBufferHeight = Constants.GbHeight * 4;
            _graphics.ApplyChanges();

            Data.Load(GraphicsDevice);

            Data.Shader.Parameters["LutTexture"].SetValue(Data.LUT);
            Data.Shader.Parameters["LutWidth"].SetValue((float)Data.LUT.Width);
            Data.Shader.Parameters["LutHeight"].SetValue((float)Data.LUT.Height);
            Data.Shader.Parameters["tone1"].SetValue(Constants.Tone1);
            Data.Shader.Parameters["tone2"].SetValue(Constants.Tone2);
            Data.Shader.Parameters["tone3"].SetValue(Constants.Tone3);
            Data.Shader.Parameters["tone4"].SetValue(Constants.Tone4);
            _pass = Data.Shader.CurrentTechnique.Passes[0];

            _backbuffer = new RenderTarget2D(GraphicsDevice, Constants.GbWidth, Constants.GbHeight);
            _shaderbuffer = new RenderTarget2D(GraphicsDevice, Constants.GbWidth, Constants.GbHeight);

            _scene = new StartScene(this);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            Audio.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var isTransitioning = UpdateTransition(gameTime);
            Audio.Update();
            Input.Update(gameTime, isTransitioning);

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

            if (Input.Pressed(Actions.Screenshot))
                TakeScreenshot(false);
            if (Input.Pressed(Actions.ScreenshotGbRes))
                TakeScreenshot(true);
        }

        private void TakeScreenshot(bool useGbRes)
        {
            var factor = useGbRes
                ? 1
                : 4;

            var buffer = new RenderTarget2D(GraphicsDevice, Constants.GbWidth * factor, Constants.GbHeight * factor);
            GraphicsDevice.SetRenderTarget(buffer);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            SpriteBatch.Draw(_shaderbuffer, new Rectangle(0, 0, Constants.GbWidth * factor, Constants.GbHeight * factor),
                Color.White);
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            buffer.Save($"{Data.SaveDir}screenshots/{Guid.NewGuid()}.png");
        }

        public void SetPalette(Color color1, Color color2, Color color3, Color color4)
        {
            Color1 = color1.ToVector4();
            Color2 = color2.ToVector4();
            Color3 = color3.ToVector4();
            Color4 = color4.ToVector4();
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(_backbuffer);
            GraphicsDevice.Clear(Color.White);

            _scene.Draw(gameTime);
            DrawTransition();

            GraphicsDevice.SetRenderTarget(_shaderbuffer);
            GraphicsDevice.Clear(Color.White);

            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp);
            Data.Shader.Parameters["col1"].SetValue(Color1);
            Data.Shader.Parameters["col2"].SetValue(Color2);
            Data.Shader.Parameters["col3"].SetValue(Color3);
            Data.Shader.Parameters["col4"].SetValue(Color4);
            _pass.Apply();
            SpriteBatch.Draw(_backbuffer, Vector2.Zero, Color.White);
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            SpriteBatch.Draw(_shaderbuffer, ScaledBackbuffer(), Color.White);
            SpriteBatch.End();
        }


        private void DrawTransition()
        {
            var phase = (int)(_transition * 8f);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            for (var x = 0; x < Constants.GbWidth / 8; x++)
            {
                for (var y = 0; y < Constants.GbHeight / 8; y++)
                {
                    SpriteBatch.Draw(Data.Wipe, new Vector2(x * 8, y * 8), new Rectangle(phase * 8, 0, 8, 8), Color.White);
                }
            }
            SpriteBatch.End();
        }

        public void Transition(IScene scene)
        {
            if (_isTransitioning)
                return;

            _isTransitioning = true;
            _transitionTarget = scene;
            _transitionDirection = false;
            _transition = 1f;
            _transitionWaitTime = 0.65f;
            _sceneClosed = false;
        }

        public bool UpdateTransition(GameTime gameTime)
        {
            if (_isTransitioning)
            {
                if (!_transitionDirection)
                {
                    if (!_sceneClosed)
                    {
                        _scene.Close();
                        _sceneClosed = true;
                    }

                    _transition -= (gameTime.GetElapsedSeconds() * 2);

                    if (_transition <= 0f)
                    {
                        _transitionWaitTime -= (gameTime.GetElapsedSeconds() * 2);

                        if (_transitionWaitTime <= 0f)
                        {
                            _scene = _transitionTarget;

                            _transitionDirection = true;
                        }
                    }
                }
                else
                {
                    _transition += (gameTime.GetElapsedSeconds() * 2);

                    if (_transition >= 1f)
                    {
                        _scene.Initialise();
                        _isTransitioning = false;
                    }
                }

                return true;
            }

            return false;
        }

        private Rectangle ScaledBackbuffer()
        {
            var displayWidth = Window.ClientBounds.Width;
            var displayHeight = Window.ClientBounds.Height;
            var width = Constants.GbWidth;
            var height = Constants.GbHeight;
            var widthScale = displayWidth / (double)Constants.GbWidth;
            var heightScale = displayHeight / (double)Constants.GbHeight;
            var smallest = (int)Math.Min(widthScale, heightScale);

            width *= smallest;
            height *= smallest;

            var x = displayWidth / 2 - width / 2;
            var y = displayHeight / 2 - height / 2;

            return new Rectangle(x, y, width, height);
        }
    }
}