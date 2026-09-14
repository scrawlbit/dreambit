using DreamBit.Engine.Audio;
using DreamBit.Engine.Project;
using DreamBit.Engine.Rendering;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class HotReloadTests
    {
        [Fact]
        public void IsAssetFile_ReconheceImagensTilemapsESons()
        {
            Assert.True(ProjectWatcher.IsAssetFile("hero.png"));
            Assert.True(ProjectWatcher.IsAssetFile(@"C:\jogo\assets\Dungeon1.TMX")); // maiúsculas
            Assert.True(ProjectWatcher.IsAssetFile("tiles.tsx"));
            Assert.True(ProjectWatcher.IsAssetFile("jump.wav"));
        }

        [Fact]
        public void IsAssetFile_IgnoraOutrosArquivos()
        {
            Assert.False(ProjectWatcher.IsAssetFile("cena.dbscene"));
            Assert.False(ProjectWatcher.IsAssetFile("readme.txt"));
            Assert.False(ProjectWatcher.IsAssetFile("musica.mp3")); // só WAV
            Assert.False(ProjectWatcher.IsAssetFile(""));
        }

        [Fact]
        public void TextureCache_InvalidateEGetSemDevice_NaoLancam()
        {
            // Sem GraphicsDevice não dá para carregar textura de verdade, mas as chamadas
            // de invalidação (que podem vir de outra thread) devem ser sempre seguras.
            TextureCache.Invalidate(null);
            TextureCache.Invalidate("nao-existe.png");
            TextureCache.Invalidate(@"C:\qualquer\caminho.png");
            Assert.Null(TextureCache.Get(null!, null));
        }

        [Fact]
        public void SoundCache_InvalidateSemArquivo_NaoLanca()
        {
            SoundCache.Invalidate(null);
            SoundCache.Invalidate("inexistente.wav");
            Assert.Null(SoundCache.Get(null));
            Assert.Null(SoundCache.Get("inexistente.wav")); // arquivo invalido -> null
        }
    }
}
