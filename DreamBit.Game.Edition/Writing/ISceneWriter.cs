using DreamBit.Game.Elements;

namespace DreamBit.Game.Writing
{
    public interface ISceneWriter
    {
        void Save(Scene scene, string assetName);
    }
}