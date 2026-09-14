using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class PlaySnapshotTests
    {
        [TestMethod]
        public void CopyFrom_RestauraOEstadoDaCena()
        {
            var scene = new Scene { Name = "Fase" };
            var obj = new GameObject("Herói");
            obj.Transform.Position = new Vector2(10, 20);
            scene.Add(obj);

            // snapshot antes de "jogar"
            string snapshot = SceneSerializer.SaveToString(scene);

            // simula o play movendo o objeto
            obj.Transform.Position = new Vector2(999, 999);

            // restaura
            scene.CopyFrom(SceneSerializer.LoadFromString(snapshot));

            var restored = scene.Objects.Single();
            Assert.AreEqual("Herói", restored.Name);
            Assert.AreEqual(10f, restored.Transform.Position.X, 0.01f);
            Assert.AreEqual(20f, restored.Transform.Position.Y, 0.01f);
            Assert.AreSame(scene, restored.Scene); // back-ref reatribuída
        }
    }
}
