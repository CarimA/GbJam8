using Microsoft.Xna.Framework;

namespace GBJamGame.Scenes
{
    public interface IScene
    {
        void Update(GameTime gameTIme);
        void Draw(GameTime gameTime);
    }
}