using System.IO;
using System.Linq;
using DreamBit.Engine.Data;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class DataCatalogTests
    {
        public sealed class Inimigo
        {
            public string Nome { get; set; } = "";
            public int Vida { get; set; }
            public float Velocidade { get; set; }
        }

        public DataCatalogTests() => DataCatalog.Clear();

        [Fact]
        public void LoadMap_ConsultaPorId()
        {
            const string json = "{\"goblin\":{\"nome\":\"Goblin\",\"vida\":10,\"velocidade\":1.5}," +
                                 "\"ogro\":{\"nome\":\"Ogro\",\"vida\":40,\"velocidade\":0.8}}";
            DataCatalog.LoadMapFromJson<Inimigo>(json);

            Assert.True(DataCatalog.Has<Inimigo>("goblin"));
            var ogro = DataCatalog.Get<Inimigo>("ogro");
            Assert.NotNull(ogro);
            Assert.Equal("Ogro", ogro!.Nome);
            Assert.Equal(40, ogro.Vida);
            Assert.Equal(2, DataCatalog.All<Inimigo>().Count());
        }

        [Fact]
        public void Get_IdInexistenteRetornaDefault()
        {
            DataCatalog.LoadMapFromJson<Inimigo>("{\"a\":{\"nome\":\"A\",\"vida\":1}}");
            Assert.Null(DataCatalog.Get<Inimigo>("nao_existe"));
            Assert.False(DataCatalog.Has<Inimigo>("nao_existe"));
        }

        [Fact]
        public void LoadList_LeArray()
        {
            var path = Path.Combine(Path.GetTempPath(), "dreambit_cat_" + System.Guid.NewGuid().ToString("N") + ".json");
            try
            {
                File.WriteAllText(path, "[{\"nome\":\"X\",\"vida\":5},{\"nome\":\"Y\",\"vida\":7}]");
                var lista = DataCatalog.LoadList<Inimigo>(path);
                Assert.Equal(2, lista.Count);
                Assert.Equal("Y", lista[1].Nome);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void CaseInsensitiveNasPropriedades()
        {
            // JSON com nomes em maiúsculas ainda mapeia (PropertyNameCaseInsensitive).
            DataCatalog.LoadMapFromJson<Inimigo>("{\"z\":{\"NOME\":\"Z\",\"VIDA\":3}}");
            Assert.Equal("Z", DataCatalog.Get<Inimigo>("z")!.Nome);
        }
    }
}
