using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Navigation;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class PathfindingTests
    {
        [Fact]
        public void CaminhoRetoSemObstaculos()
        {
            var blocked = new bool[5, 1];
            var path = Pathfinding.FindPath(blocked, new Point(0, 0), new Point(4, 0));
            Assert.Equal(5, path.Count);
            Assert.Equal(new Point(0, 0), path[0]);
            Assert.Equal(new Point(4, 0), path[^1]);
        }

        [Fact]
        public void DesviaDeParede()
        {
            // Parede vertical em x=1 nas linhas 0..2, deixando passagem em y=3.
            var blocked = new bool[3, 4];
            blocked[1, 0] = true; blocked[1, 1] = true; blocked[1, 2] = true;

            var path = Pathfinding.FindPath(blocked, new Point(0, 0), new Point(2, 0));
            Assert.True(path.Count > 0, "deve achar caminho contornando");
            Assert.False(path.Any(p => blocked[p.X, p.Y]), "não passa por células bloqueadas");
            Assert.Equal(new Point(2, 0), path[^1]);
        }

        [Fact]
        public void SemCaminhoRetornaVazio()
        {
            // Objetivo cercado.
            var blocked = new bool[3, 3];
            blocked[1, 0] = true; blocked[0, 1] = true; blocked[1, 1] = true;
            // alvo (2,2) alcançável na verdade; vamos cercar (0,0) em vez disso.
            blocked = new bool[3, 3];
            blocked[1, 0] = true; blocked[0, 1] = true; blocked[1, 1] = true;
            var path = Pathfinding.FindPath(blocked, new Point(0, 0), new Point(2, 2));
            Assert.Empty(path);
        }

        [Fact]
        public void DiagonalNaoCortaQuina()
        {
            // Bloqueios em (1,0) e (0,1): a diagonal (0,0)->(1,1) não pode cortar a quina.
            var blocked = new bool[2, 2];
            blocked[1, 0] = true; blocked[0, 1] = true;
            var path = Pathfinding.FindPath(blocked, new Point(0, 0), new Point(1, 1), allowDiagonal: true);
            Assert.Empty(path);
        }

        [Fact]
        public void NavGrid_DoTilemapAchaCaminhoNoMundo()
        {
            var obj = new GameObject("Mapa");
            var map = new Tilemap.Tilemap { TileWidth = 32, TileHeight = 32 };
            var solido = new Tilemap.TileLayer { Name = "Solido" };
            // parede vertical em coluna x=1, linhas 0..2
            solido.SetTile(1, 0, 1); solido.SetTile(1, 1, 1); solido.SetTile(1, 2, 1);
            map.Layers.Add(solido);
            obj.AddComponent(new TilemapRenderer { Map = map, Solid = true, SolidLayer = "Solido" });

            var grid = NavGrid.FromTilemap(obj.Components.OfType<TilemapRenderer>().Single(), margin: 2);
            Assert.NotNull(grid);

            var start = new Vector2(0 * 32 + 16, 0 * 32 + 16);   // célula (0,0)
            var goal = new Vector2(2 * 32 + 16, 0 * 32 + 16);    // célula (2,0), do outro lado
            var path = grid!.FindPath(start, goal);

            Assert.True(path.Count > 0, "achou caminho no mundo");
            Assert.Equal(goal, path[^1]);
        }
    }
}
