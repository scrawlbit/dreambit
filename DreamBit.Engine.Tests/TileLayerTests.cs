using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class TileLayerTests
    {
        [Fact]
        public void AddLayer_NomeiaEAcumula()
        {
            var map = new Tilemap.Tilemap();
            var a = map.AddLayer();
            var b = map.AddLayer("Colisão");

            Assert.Equal(2, map.Layers.Count);
            Assert.Equal("Camada 1", a.Name);
            Assert.Equal("Colisão", b.Name);
            Assert.True(a.Visible);
        }

        [Fact]
        public void Serializacao_PreservaCamadasEVisibilidade()
        {
            var scene = new Scene();
            var obj = new GameObject("Mapa");
            var map = new Tilemap.Tilemap { TileWidth = 16, TileHeight = 16 };
            var fundo = map.AddLayer("Fundo"); fundo.SetTile(0, 0, 1);
            var frente = map.AddLayer("Frente"); frente.SetTile(1, 1, 2); frente.Visible = false;
            obj.AddComponent(new TilemapRenderer { Map = map, Edited = true });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var lmap = loaded.Objects.First().Components.OfType<TilemapRenderer>().Single().Map!;

            Assert.Equal(2, lmap.Layers.Count);
            Assert.Equal("Fundo", lmap.Layers[0].Name);
            Assert.True(lmap.Layers[0].Visible);
            Assert.Equal("Frente", lmap.Layers[1].Name);
            Assert.False(lmap.Layers[1].Visible, "visibilidade da camada sobrevive");
            Assert.Equal(2, lmap.Layers[1].GetTile(1, 1));
        }

        [Fact]
        public void CamadaInvisivelNaoContaColisao_MasVisivelSim()
        {
            // A colisão (Solid) independe da visibilidade: usa as células, não o desenho.
            var obj = new GameObject("Chao");
            var map = new Tilemap.Tilemap { TileWidth = 32, TileHeight = 32 };
            var solido = map.AddLayer("Solido");
            solido.SetTile(0, 0, 1);
            solido.Visible = false; // escondida no editor, ainda colide
            obj.AddComponent(new TilemapRenderer { Map = map, Solid = true, SolidLayer = "Solido" });

            var boxes = obj.Components.OfType<TilemapRenderer>().Single().SolidBoxes().ToList();
            Assert.Single(boxes);
        }
    }
}
