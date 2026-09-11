using DreamBit.Engine.Elements;

namespace DreamBit.Engine.Scripting
{
    /// <summary>
    /// Interface que um script do usuário implementa. O componente Script compila o
    /// código em runtime (Roslyn) e chama <see cref="Update"/> a cada frame no play.
    /// </summary>
    public interface IGameScript
    {
        void Update(GameObject self, float deltaSeconds);
    }
}
