using DreamBit.Project;
using MonoGameContentManager = Microsoft.Xna.Framework.Content.ContentManager;

namespace DreamBit.Game.Content
{
    public interface IContentLoader
    {
        MonoGameContentManager Manager { get; set; }

        T Load<T>(IProjectFile file);
    }
}
