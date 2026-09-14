using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class LockedSerializationTests
    {
        [Fact]
        public void Locked_RoundTrip_Preservado()
        {
            var scene = new Scene();
            scene.Add(new GameObject("Travado") { Locked = true });
            scene.Add(new GameObject("Livre"));

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));

            Assert.True(loaded.Objects[0].Locked);
            Assert.False(loaded.Objects[1].Locked);
        }
    }
}
