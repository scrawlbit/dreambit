using System.Linq;
using System.Xml.Linq;
using DreamBit.Engine.Tilemap;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class TmxImporterTests
    {
        [Fact]
        public void Parse_MapaFinito_LeTilesetsELayers()
        {
            const string xml = @"<map tilewidth='16' tileheight='16'>
              <tileset firstgid='1' name='ts' tilewidth='16' tileheight='16' columns='4' tilecount='16'>
                <image source='ts.png' width='64' height='64'/>
              </tileset>
              <layer name='L1' width='2' height='2'>
                <data encoding='csv'>1,2,0,3</data>
              </layer>
            </map>";

            var map = TmxImporter.Parse(XDocument.Parse(xml));

            Assert.Equal(16, map.TileWidth);
            Assert.Single(map.Tilesets);
            Assert.Equal(4, map.Tilesets[0].Columns);
            Assert.Equal("ts.png", map.Tilesets[0].ImageSource);

            var tiles = map.Layers.Single().Tiles;
            CollectionAssert.AreEquivalent(
                new[] { (0, 0, 1), (1, 0, 2), (1, 1, 3) },
                tiles.ToList());
        }

        [Fact]
        public void Parse_MapaInfinito_JuntaOsChunks()
        {
            const string xml = @"<map tilewidth='16' tileheight='16' infinite='1'>
              <tileset firstgid='1' columns='2' tilecount='4'><image source='a.png'/></tileset>
              <layer name='L'>
                <data encoding='csv'>
                  <chunk x='0' y='0' width='2' height='2'>1,0,0,0</chunk>
                  <chunk x='2' y='0' width='2' height='2'>0,0,0,2</chunk>
                </data>
              </layer>
            </map>";

            var map = TmxImporter.Parse(XDocument.Parse(xml));
            var tiles = map.Layers.Single().Tiles;

            CollectionAssert.AreEquivalent(
                new[] { (0, 0, 1), (3, 1, 2) },
                tiles.ToList());
        }

        [Fact]
        public void TilesetForGid_EscolheAFaixaCorreta()
        {
            var map = new Tilemap.Tilemap();
            map.Tilesets.Add(new Tileset { FirstGid = 1, Columns = 2 });
            map.Tilesets.Add(new Tileset { FirstGid = 100, Columns = 2 });

            Assert.Equal(1, map.TilesetForGid(50)!.FirstGid);
            Assert.Equal(100, map.TilesetForGid(150)!.FirstGid);
        }
    }
}
