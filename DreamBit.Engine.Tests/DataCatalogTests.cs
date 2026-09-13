using System.IO;
using System.Linq;
using DreamBit.Engine.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class DataCatalogTests
    {
        public sealed class Inimigo
        {
            public string Nome { get; set; } = "";
            public int Vida { get; set; }
            public float Velocidade { get; set; }
        }

        [TestInitialize]
        public void Reset() => DataCatalog.Clear();

        [TestMethod]
        public void LoadMap_ConsultaPorId()
        {
            const string json = "{\"goblin\":{\"nome\":\"Goblin\",\"vida\":10,\"velocidade\":1.5}," +
                                 "\"ogro\":{\"nome\":\"Ogro\",\"vida\":40,\"velocidade\":0.8}}";
            DataCatalog.LoadMapFromJson<Inimigo>(json);

            Assert.IsTrue(DataCatalog.Has<Inimigo>("goblin"));
            var ogro = DataCatalog.Get<Inimigo>("ogro");
            Assert.IsNotNull(ogro);
            Assert.AreEqual("Ogro", ogro!.Nome);
            Assert.AreEqual(40, ogro.Vida);
            Assert.AreEqual(2, DataCatalog.All<Inimigo>().Count());
        }

        [TestMethod]
        public void Get_IdInexistenteRetornaDefault()
        {
            DataCatalog.LoadMapFromJson<Inimigo>("{\"a\":{\"nome\":\"A\",\"vida\":1}}");
            Assert.IsNull(DataCatalog.Get<Inimigo>("nao_existe"));
            Assert.IsFalse(DataCatalog.Has<Inimigo>("nao_existe"));
        }

        [TestMethod]
        public void LoadList_LeArray()
        {
            var path = Path.Combine(Path.GetTempPath(), "dreambit_cat_" + System.Guid.NewGuid().ToString("N") + ".json");
            try
            {
                File.WriteAllText(path, "[{\"nome\":\"X\",\"vida\":5},{\"nome\":\"Y\",\"vida\":7}]");
                var lista = DataCatalog.LoadList<Inimigo>(path);
                Assert.AreEqual(2, lista.Count);
                Assert.AreEqual("Y", lista[1].Nome);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [TestMethod]
        public void CaseInsensitiveNasPropriedades()
        {
            // JSON com nomes em maiúsculas ainda mapeia (PropertyNameCaseInsensitive).
            DataCatalog.LoadMapFromJson<Inimigo>("{\"z\":{\"NOME\":\"Z\",\"VIDA\":3}}");
            Assert.AreEqual("Z", DataCatalog.Get<Inimigo>("z")!.Nome);
        }
    }
}
