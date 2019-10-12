using DreamBit.Game.Files;
using DreamBit.Project;
using DreamBit.Project.Registrations;

namespace DreamBit.Game.Registrations
{
    internal interface ISceneFileRegistration : IFileRegistration { }
    internal class SceneFileRegistration : ISceneFileRegistration
    {
        public string Type => "Scene";
        public string Extension => ".scene";

        public ProjectFile CreateInstance()
        {
            return new SceneFile();
        }
    }
}
