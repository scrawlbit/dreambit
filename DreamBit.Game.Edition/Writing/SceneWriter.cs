using System.Text;
using DreamBit.Game.Elements;

namespace DreamBit.Game.Writing
{
    public class SceneWriter : ISceneWriter
    {
        private readonly IDataWriter _dataWriter;
        private readonly UTF8Encoding _encoding;

        public SceneWriter() : this(EditionContainer.Resolve<IDataWriter>())
        {
        }
        internal SceneWriter(IDataWriter dataWriter)
        {
            _dataWriter = dataWriter;
            _encoding = new UTF8Encoding(false);
        }

        public void Save(Scene scene, string assetName)
        {
            _dataWriter.Save(scene, assetName, "scene", _encoding);
        }
    }
}