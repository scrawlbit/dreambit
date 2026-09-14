using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Pooling;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class ObjectPoolTests
    {
        private static GameObject Template()
        {
            var t = new GameObject("Bala");
            t.AddComponent(new SpriteRenderer { Size = new Vector2(8, 8) });
            return t;
        }

        [Fact]
        public void Prewarm_CriaReservaSemAtivar()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template(), prewarm: 3);

            Assert.Equal(3, pool.FreeCount);
            Assert.Equal(0, pool.ActiveCount);
            Assert.Empty(scene.Objects);
        }

        [Fact]
        public void GetAtivaEReturnDevolve()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template(), prewarm: 1);

            var a = pool.Get();
            Assert.Equal(1, pool.ActiveCount);
            Assert.Equal(0, pool.FreeCount);
            Assert.Contains(a, scene.Objects);

            Assert.True(pool.Return(a));
            Assert.Equal(0, pool.ActiveCount);
            Assert.Equal(1, pool.FreeCount);
            Assert.DoesNotContain(a, scene.Objects);
        }

        [Fact]
        public void ReutilizaAMesmaInstancia()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template(), prewarm: 1);

            var a = pool.Get();
            pool.Return(a);
            var b = pool.Get();

            Assert.Same(a, b);
        }

        [Fact]
        public void CloneEhIndependenteComNovoId()
        {
            var scene = new Scene();
            var template = Template();
            var pool = new ObjectPool(scene, template, prewarm: 0);

            var a = pool.Get();
            Assert.NotEqual(template.Id, a.Id);
            Assert.Single(a.Components.OfType<SpriteRenderer>());
            Assert.NotSame(template.Components.First(), a.Components.First());
        }

        [Fact]
        public void ReturnDeObjetoAlheioNaoAfeta()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template());
            var estranho = new GameObject("X");

            Assert.False(pool.Return(estranho));
        }

        [Fact]
        public void ReturnAll_DevolveTudo()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template());
            pool.Get(); pool.Get(); pool.Get();
            Assert.Equal(3, pool.ActiveCount);

            pool.ReturnAll();
            Assert.Equal(0, pool.ActiveCount);
            Assert.Equal(3, pool.FreeCount);
            Assert.Empty(scene.Objects);
        }
    }
}
