namespace DreamBit.Game.Reading
{
    internal interface IDataReader
    {
        T Load<T>(string assetName, string extension);
    }
}