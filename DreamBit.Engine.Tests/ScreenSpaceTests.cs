using System.Linq;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class ScreenSpaceTests
    {
        [Fact]
        public void FilhoHerdaScreenSpaceDoAncestral()
        {
            var painel = new GameObject("HUD") { ScreenSpace = true };
            var icone = new GameObject("Vida");
            painel.AddChild(icone);

            Assert.True(painel.EffectiveScreenSpace);
            Assert.True(icone.EffectiveScreenSpace, "filho de objeto HUD também é HUD");
            Assert.False(icone.ScreenSpace, "o flag próprio do filho continua false");
        }

        [Fact]
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

        [Fact]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            scene.Add(new GameObject("HUD") { ScreenSpace = true });
            scene.Add(new GameObject("Mundo"));

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            Assert.True(loaded.Objects.First(o => o.Name == "HUD").ScreenSpace);
            Assert.False(loaded.Objects.First(o => o.Name == "Mundo").ScreenSpace);
        }
    }
}
