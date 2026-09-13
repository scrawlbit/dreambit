using System.IO;
using DreamBit.Engine.Saving;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class SaveGameTests
    {
        [TestInitialize]
        public void Reset() => SaveGame.Clear();

        [TestMethod]
        public void LeituraTipadaComDefaults()
        {
            Assert.AreEqual(3, SaveGame.GetInt("vidas", 3), "chave ausente => fallback");

            SaveGame.SetInt("vidas", 5);
            SaveGame.SetFloat("volume", 0.8f);
            SaveGame.SetBool("tutorial", true);
            SaveGame.SetString("nome", "Erick");

            Assert.AreEqual(5, SaveGame.GetInt("vidas"));
            Assert.AreEqual(0.8f, SaveGame.GetFloat("volume"), 0.0001f);
            Assert.IsTrue(SaveGame.GetBool("tutorial"));
            Assert.AreEqual("Erick", SaveGame.GetString("nome"));
            Assert.IsTrue(SaveGame.Has("vidas"));

            SaveGame.Delete("vidas");
            Assert.IsFalse(SaveGame.Has("vidas"));
        }

        [TestMethod]
        public void SalvarECarregarArquivo()
        {
            var path = Path.Combine(Path.GetTempPath(), "dreambit_save_test_" + System.Guid.NewGuid().ToString("N") + ".json");
            try
            {
                SaveGame.SetInt("fase", 2);
                SaveGame.SetString("jogador", "Aria");
                SaveGame.Save(path);
                Assert.IsTrue(SaveGame.Exists(path));

                SaveGame.Clear();
                Assert.AreEqual(0, SaveGame.GetInt("fase"));

                Assert.IsTrue(SaveGame.Load(path));
                Assert.AreEqual(2, SaveGame.GetInt("fase"));
                Assert.AreEqual("Aria", SaveGame.GetString("jogador"));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [TestMethod]
        public void CarregarInexistenteRetornaFalse()
        {
            var path = Path.Combine(Path.GetTempPath(), "nao_existe_" + System.Guid.NewGuid().ToString("N") + ".json");
            Assert.IsFalse(SaveGame.Load(path));
        }

        [TestMethod]
        public void FloatUsaCulturaInvariante()
        {
            // Garante que decimais não quebram em locais que usam vírgula (pt-BR).
            var path = Path.Combine(Path.GetTempPath(), "dreambit_culture_" + System.Guid.NewGuid().ToString("N") + ".json");
            try
            {
                SaveGame.SetFloat("x", 1.25f);
                SaveGame.Save(path);
                SaveGame.Clear();
                SaveGame.Load(path);
                Assert.AreEqual(1.25f, SaveGame.GetFloat("x"), 0.0001f);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
