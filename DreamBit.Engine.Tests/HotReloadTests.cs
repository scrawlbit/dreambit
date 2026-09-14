using DreamBit.Engine.Audio;
using DreamBit.Engine.Project;
using DreamBit.Engine.Rendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class HotReloadTests
    {
        [TestMethod]
        public void IsAssetFile_ReconheceImagensTilemapsESons()
        {
            Assert.IsTrue(ProjectWatcher.IsAssetFile("hero.png"));
            Assert.IsTrue(ProjectWatcher.IsAssetFile(@"C:\jogo\assets\Dungeon1.TMX")); // maiúsculas
            Assert.IsTrue(ProjectWatcher.IsAssetFile("tiles.tsx"));
            Assert.IsTrue(ProjectWatcher.IsAssetFile("jump.wav"));
        }

        [TestMethod]
        public void IsAssetFile_IgnoraOutrosArquivos()
        {
            Assert.IsFalse(ProjectWatcher.IsAssetFile("cena.dbscene"));
            Assert.IsFalse(ProjectWatcher.IsAssetFile("readme.txt"));
            Assert.IsFalse(ProjectWatcher.IsAssetFile("musica.mp3")); // só WAV
            Assert.IsFalse(ProjectWatcher.IsAssetFile(""));
        }

        [TestMethod]
        public void TextureCache_InvalidateEGetSemDevice_NaoLancam()
        {
            // Sem GraphicsDevice não dá para carregar textura de verdade, mas as chamadas
            // de invalidação (que podem vir de outra thread) devem ser sempre seguras.
            TextureCache.Invalidate(null);
            TextureCache.Invalidate("nao-existe.png");
            TextureCache.Invalidate(@"C:\qualquer\caminho.png");
            Assert.IsNull(TextureCache.Get(null!, null));
        }

        [TestMethod]
        public void SoundCache_InvalidateSemArquivo_NaoLanca()
        {
            SoundCache.Invalidate(null);
            SoundCache.Invalidate("inexistente.wav");
            Assert.IsNull(SoundCache.Get(null));
            Assert.IsNull(SoundCache.Get("inexistente.wav")); // arquivo invalido -> null
        }
    }
}
