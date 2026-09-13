using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Pooling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class ObjectPoolTests
    {
        private static GameObject Template()
        {
            var t = new GameObject("Bala");
            t.AddComponent(new SpriteRenderer { Size = new Vector2(8, 8) });
            return t;
        }

        [TestMethod]
        public void Prewarm_CriaReservaSemAtivar()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template(), prewarm: 3);

            Assert.AreEqual(3, pool.FreeCount);
            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(0, scene.Objects.Count, "reserva não entra na cena");
        }

        [TestMethod]
        public void GetAtivaEReturnDevolve()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template(), prewarm: 1);

            var a = pool.Get();
            Assert.AreEqual(1, pool.ActiveCount);
            Assert.AreEqual(0, pool.FreeCount);
            Assert.IsTrue(scene.Objects.Contains(a));

            Assert.IsTrue(pool.Return(a));
            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(1, pool.FreeCount);
            Assert.IsFalse(scene.Objects.Contains(a));
        }

        [TestMethod]
        public void ReutilizaAMesmaInstancia()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template(), prewarm: 1);

            var a = pool.Get();
            pool.Return(a);
            var b = pool.Get();

            Assert.AreSame(a, b, "sem alocar novo enquanto há reserva");
        }

        [TestMethod]
        public void CloneEhIndependenteComNovoId()
        {
            var scene = new Scene();
            var template = Template();
            var pool = new ObjectPool(scene, template, prewarm: 0);

            var a = pool.Get();
            Assert.AreNotEqual(template.Id, a.Id, "clone recebe novo Id");
            Assert.AreEqual(1, a.Components.OfType<SpriteRenderer>().Count(), "componentes clonados");
            Assert.AreNotSame(template.Components.First(), a.Components.First());
        }

        [TestMethod]
        public void ReturnDeObjetoAlheioNaoAfeta()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template());
            var estranho = new GameObject("X");

            Assert.IsFalse(pool.Return(estranho));
        }

        [TestMethod]
        public void ReturnAll_DevolveTudo()
        {
            var scene = new Scene();
            var pool = new ObjectPool(scene, Template());
            pool.Get(); pool.Get(); pool.Get();
            Assert.AreEqual(3, pool.ActiveCount);

            pool.ReturnAll();
            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(3, pool.FreeCount);
            Assert.AreEqual(0, scene.Objects.Count);
        }
    }
}
