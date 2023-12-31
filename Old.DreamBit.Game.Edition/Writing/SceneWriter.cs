﻿using System.Text;
using DreamBit.Game.Elements;

namespace DreamBit.Game.Writing
{
    internal class SceneWriter : ISceneWriter
    {
        private readonly IDataWriter _dataWriter;
        private readonly UTF8Encoding _encoding;

        public SceneWriter(IDataWriter dataWriter)
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