using System.Text;

namespace DreamBit.Game.Writing
{
    internal interface IDataWriter
    {
        void Save<T>(T data, string assetName, string extension, Encoding encoding);
    }
}