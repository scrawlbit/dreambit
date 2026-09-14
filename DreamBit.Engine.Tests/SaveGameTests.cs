using System.IO;
using DreamBit.Engine.Saving;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class SaveGameTests
    {
        public SaveGameTests() => SaveGame.Clear();

        [Fact]
        public void LeituraTipadaComDefaults()
        {
            Assert.Equal(3, SaveGame.GetInt("vidas", 3));

            SaveGame.SetInt("vidas", 5);
            SaveGame.SetFloat("volume", 0.8f);
            SaveGame.SetBool("tutorial", true);
            SaveGame.SetString("nome", "Erick");

            Assert.Equal(5, SaveGame.GetInt("vidas"));
            Assert.Equal(0.8f, SaveGame.GetFloat("volume"), 0.0001f);
            Assert.True(SaveGame.GetBool("tutorial"));
            Assert.Equal("Erick", SaveGame.GetString("nome"));
            Assert.True(SaveGame.Has("vidas"));

            SaveGame.Delete("vidas");
            Assert.False(SaveGame.Has("vidas"));
        }

        [Fact]
        public void SalvarECarregarArquivo()
        {
            var path = Path.Combine(Path.GetTempPath(), "dreambit_save_test_" + System.Guid.NewGuid().ToString("N") + ".json");
            try
            {
                SaveGame.SetInt("fase", 2);
                SaveGame.SetString("jogador", "Aria");
                SaveGame.Save(path);
                Assert.True(SaveGame.Exists(path));

                SaveGame.Clear();
                Assert.Equal(0, SaveGame.GetInt("fase"));

                Assert.True(SaveGame.Load(path));
                Assert.Equal(2, SaveGame.GetInt("fase"));
                Assert.Equal("Aria", SaveGame.GetString("jogador"));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void CarregarInexistenteRetornaFalse()
        {
            var path = Path.Combine(Path.GetTempPath(), "nao_existe_" + System.Guid.NewGuid().ToString("N") + ".json");
            Assert.False(SaveGame.Load(path));
        }

        [Fact]
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
                Assert.Equal(1.25f, SaveGame.GetFloat("x"), 0.0001f);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
