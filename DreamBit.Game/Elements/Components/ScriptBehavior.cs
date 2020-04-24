using DreamBit.Game.Files;

namespace DreamBit.Game.Elements.Components
{
    public class ScriptBehavior : GameComponent
    {
        internal ScriptBehavior()
        {

        }
        public ScriptBehavior(IScriptFile file)
        {
            File = file;
        }

        public IScriptFile File { get; internal set; }
        public override string Name => File.Name;
    }
}
