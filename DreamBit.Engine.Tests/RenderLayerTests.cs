using System;
using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class RenderLayerTests
    {
        private static GameTime Frame => new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

        [Fact]
        public void CamadaOrdenaAntesDoZOrder()
        {
            var scene = new Scene();
            var frente = new GameObject("Frente") { RenderLayer = 100, SortOrder = -999 };
            var fundo = new GameObject("Fundo") { RenderLayer = -100, SortOrder = 999 };
            var meio = new GameObject("Meio") { RenderLayer = 0, SortOrder = 0 };
            scene.Add(frente);
            scene.Add(fundo);
            scene.Add(meio);

            var order = scene.VisibleInDrawOrder().Select(o => o.Name).ToList();
            CollectionAssert.AreEqual(new[] { "Fundo", "Meio", "Frente" }, order);
        }

        [Fact]
        public void Parallax_RolaMaisDevagarQueACamera()
        {
            var scene = new Scene();
            var bg = new GameObject("Fundo");
            bg.Transform.Position = new Vector2(0, 0);
            bg.AddComponent(new ParallaxLayer { FactorX = 0.25f, FactorY = 1f });
            scene.Add(bg);

            scene.StartPlay();
            Screen.CameraPosition = new Vector2(400, 100);
            scene.Update(Frame);

            // X: base(0) + cam.X * (1 - 0.25) = 300 ; Y: fator 1 => sem deslocamento
            Assert.Equal(300f, bg.Transform.Position.X, 0.01f);
            Assert.Equal(0f, bg.Transform.Position.Y, 0.01f);
        }

        [Fact]
        public void Parallax_FatorZeroAcompanhaACamera()
        {
            var scene = new Scene();
            var bg = new GameObject("CeuFixo");
            bg.Transform.Position = new Vector2(10, 20);
            bg.AddComponent(new ParallaxLayer { FactorX = 0f, FactorY = 0f });
            scene.Add(bg);

            scene.StartPlay();
            Screen.CameraPosition = new Vector2(500, 300);
            scene.Update(Frame);

            Assert.Equal(new Vector2(510, 320), bg.Transform.Position);
        }

        [Fact]
        public void Serializacao_RoundTrip()
        {
            var scene = new Scene();
            var obj = new GameObject("Fundo") { RenderLayer = -50 };
            obj.AddComponent(new ParallaxLayer { FactorX = 0.3f, FactorY = 0.7f });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var lo = loaded.Objects.First();
            Assert.Equal(-50, lo.RenderLayer);
            var px = lo.Components.OfType<ParallaxLayer>().Single();
            Assert.Equal(0.3f, px.FactorX, 0.001f);
            Assert.Equal(0.7f, px.FactorY, 0.001f);
        }
    }
}
