using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class SceneTests
    {
        [Fact]
        public void AddChild_AtualizaPaiEHierarquia()
        {
            var parent = new GameObject("pai");
            var child = new GameObject("filho");

            parent.AddChild(child);

            Assert.Same(parent, child.Parent);
            Assert.Contains(child, parent.Children);
        }

        [Fact]
        public void Reparent_RemoveDoPaiAnterior()
        {
            var a = new GameObject("a");
            var b = new GameObject("b");
            var child = new GameObject("filho");

            a.AddChild(child);
            b.AddChild(child);

            Assert.DoesNotContain(child, a.Children);
            Assert.Contains(child, b.Children);
            Assert.Same(b, child.Parent);
        }

        [Fact]
        public void AddComponent_DefineOwner()
        {
            var obj = new GameObject("obj");
            var sprite = obj.AddComponent(new SpriteRenderer());

            Assert.Same(obj, sprite.Owner);
        }

        [Fact]
        public void Serializacao_PreservaEstrutura_E_Valores()
        {
            var scene = new Scene { Name = "Teste" };
            var root = new GameObject("Raiz");
            root.Transform.Position = new Vector2(12, -8);
            root.Transform.Rotation = 0.5f;
            root.AddComponent(new SpriteRenderer { Size = new Vector2(64, 32), Color = new Color(10, 20, 30) });
            root.AddComponent(new RotatorBehavior { Speed = 2.5f });

            var child = new GameObject("Filho");
            child.Transform.Position = new Vector2(5, 5);
            root.AddChild(child);
            scene.Add(root);

            var path = Path.Combine(Path.GetTempPath(), "dreambit_test_scene.dbscene");
            try
            {
                SceneSerializer.Save(scene, path);
                var loaded = SceneSerializer.Load(path);

                Assert.Equal("Teste", loaded.Name);
                Assert.Single(loaded.Objects);

                var loadedRoot = loaded.Objects[0];
                Assert.Equal("Raiz", loadedRoot.Name);
                Assert.Equal(12f, loadedRoot.Transform.Position.X, 0.01f);
                Assert.Equal(0.5f, loadedRoot.Transform.Rotation, 0.01f);

                var sprite = loadedRoot.Components.OfType<SpriteRenderer>().Single();
                Assert.Equal(64f, sprite.Size.X, 0.01f);
                Assert.Equal((byte)20, sprite.Color.G);

                var rotator = loadedRoot.Components.OfType<RotatorBehavior>().Single();
                Assert.Equal(2.5f, rotator.Speed, 0.01f);

                Assert.Single(loadedRoot.Children);
                Assert.Equal("Filho", loadedRoot.Children[0].Name);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
