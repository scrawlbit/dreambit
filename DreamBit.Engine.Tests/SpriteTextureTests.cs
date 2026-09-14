using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class SpriteTextureTests
    {
        [Fact]
        public void Serializacao_PreservaOCaminhoDaTextura()
        {
            var scene = new Scene();
            var obj = new GameObject("Com textura");
            obj.AddComponent(new SpriteRenderer { TexturePath = @"C:\assets\hero.png" });
            scene.Add(obj);

            var path = Path.Combine(Path.GetTempPath(), "dreambit_tex_test.dbscene");
            try
            {
                SceneSerializer.Save(scene, path);
                var loaded = SceneSerializer.Load(path);

                var sprite = loaded.Objects[0].Components.OfType<SpriteRenderer>().Single();
                Assert.Equal(@"C:\assets\hero.png", sprite.TexturePath);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
