using DreamBit.Game.Reading;

namespace DreamBit.Game.Tests.Mocks.Reading
{
    public class DataReaderMock : IDataReader
    {
        public object Content { get; set; }
        public string AssetRequested { get; set; }

        public T Load<T>(string assetName, string extension)
        {
            AssetRequested = $"{assetName}.{extension}";

            return (T)Content;
        }
    }
}