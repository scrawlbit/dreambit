namespace DreamBit.Extension.Models
{
    public interface IEditingScene
    {
        ISceneObjectCollection Objects { get; }
    }

    public class EditingScene : IEditingScene
    {
        public EditingScene()
        {
            Objects = new SceneObjectCollection(null);
        }

        public ISceneObjectCollection Objects { get; }
    }
}