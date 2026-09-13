using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class ScreenSpaceTests
    {
        [TestMethod]
        public void FilhoHerdaScreenSpaceDoAncestral()
        {
            var painel = new GameObject("HUD") { ScreenSpace = true };
            var icone = new GameObject("Vida");
            painel.AddChild(icone);

            Assert.IsTrue(painel.EffectiveScreenSpace);
            Assert.IsTrue(icone.EffectiveScreenSpace, "filho de objeto HUD também é HUD");
            Assert.IsFalse(icone.ScreenSpace, "o flag próprio do filho continua false");
        }

        [TestMethod]
        public void PasseDeMundoEDeTelaSeparamOsObjetos()
        {
            var scene = new Scene();
            var mundo = new GameObject("Chao");
            var hud = new GameObject("Pontuacao") { ScreenSpace = true };
            scene.Add(mundo);
            scene.Add(hud);

            var mundoIds = scene.VisibleInDrawOrder().Where(o => !o.EffectiveScreenSpace).ToList();
            var hudIds = scene.VisibleInDrawOrder().Where(o => o.EffectiveScreenSpace).ToList();

            CollectionAssert.Contains(mundoIds, mundo);
            CollectionAssert.DoesNotContain(mundoIds, hud);
            CollectionAssert.Contains(hudIds, hud);
        }

        [TestMethod]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            scene.Add(new GameObject("HUD") { ScreenSpace = true });
            scene.Add(new GameObject("Mundo"));

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            Assert.IsTrue(loaded.Objects.First(o => o.Name == "HUD").ScreenSpace);
            Assert.IsFalse(loaded.Objects.First(o => o.Name == "Mundo").ScreenSpace);
        }
    }
}
