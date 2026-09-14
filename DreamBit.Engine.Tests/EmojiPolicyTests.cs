using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace DreamBit.Engine.Tests
{
    /// <summary>
    /// Política do projeto: sem emojis no código. Ícones da UI usam Material (PathIcon).
    /// Este teste varre .cs/.axaml e falha se algum emoji for introduzido — vetando novas adições.
    /// (Setas tipográficas → ↔ e o sinal × não são emojis e são permitidos.)
    /// </summary>
    public class EmojiPolicyTests
    {
        private static bool IsEmoji(int cp) =>
            (cp >= 0x1F000 && cp <= 0x1FAFF) || // pictogramas, emoticons, transporte, etc.
            (cp >= 0x2600 && cp <= 0x27BF) ||   // simbolos diversos + dingbats
            cp == 0xFE0F ||                     // seletor de variação emoji
            cp == 0x200D;                       // zero-width joiner (emojis compostos)

        [Fact]
        public void SemEmojisNoCodigo()
        {
            var root = FindRepoRoot();
            var offenders = new List<string>();

            foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
            {
                if (!(file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                      file.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase)))
                    continue;
                if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") ||
                    file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                    continue;

                foreach (var rune in File.ReadAllText(file).EnumerateRunes())
                    if (IsEmoji(rune.Value))
                    {
                        offenders.Add($"{Path.GetFileName(file)}: U+{rune.Value:X4}");
                        break;
                    }
            }

            Assert.True(offenders.Count == 0,
                "Emojis não são permitidos (use ícones Material). Encontrados em:\n" + string.Join("\n", offenders));
        }

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && dir.GetFiles("DreamBit.Studio.slnx").Length == 0)
                dir = dir.Parent;
            return dir?.FullName ?? AppContext.BaseDirectory;
        }
    }
}
