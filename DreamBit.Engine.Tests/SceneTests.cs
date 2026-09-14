using System.IO;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class SceneTests
    {
        [TestMethod]
        public void AddChild_AtualizaPaiEHierarquia()
        {
            var parent = new GameObject("pai");
            var child = new GameObject("filho");

            parent.AddChild(child);

            Assert.AreSame(parent, child.Parent);
            Assert.IsTrue(parent.Children.Contains(child));
        }

        [TestMethod]
        public void Reparent_RemoveDoPaiAnterior()
        {
            var a = new GameObject("a");
            var b = new GameObject("b");
            var child = new GameObject("filho");

            a.AddChild(child);
            b.AddChild(child);

            Assert.IsFalse(a.Children.Contains(child));
            Assert.IsTrue(b.Children.Contains(child));
            Assert.AreSame(b, child.Parent);
        }

        [TestMethod]
        public void AddComponent_DefineOwner()
        {
            var obj = new GameObject("obj");
            var sprite = obj.AddComponent(new SpriteRenderer());

            Assert.AreSame(obj, sprite.Owner);
        }

        [TestMethod]
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

                Assert.AreEqual("Teste", loaded.Name);
                Assert.AreEqual(1, loaded.Objects.Count);

                var loadedRoot = loaded.Objects[0];
                Assert.AreEqual("Raiz", loadedRoot.Name);
                Assert.AreEqual(12f, loadedRoot.Transform.Position.X, 0.01f);
                Assert.AreEqual(0.5f, loadedRoot.Transform.Rotation, 0.01f);

                var sprite = loadedRoot.Components.OfType<SpriteRenderer>().Single();
                Assert.AreEqual(64f, sprite.Size.X, 0.01f);
                Assert.AreEqual((byte)20, sprite.Color.G);

                var rotator = loadedRoot.Components.OfType<RotatorBehavior>().Single();
                Assert.AreEqual(2.5f, rotator.Speed, 0.01f);

                Assert.AreEqual(1, loadedRoot.Children.Count);
                Assert.AreEqual("Filho", loadedRoot.Children[0].Name);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
