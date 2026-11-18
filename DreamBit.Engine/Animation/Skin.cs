using System.Collections.Generic;

namespace DreamBit.Engine.Animation
{
    /// <summary>
    /// Skin de um esqueleto: troca todas as partes pintadas de uma vez (mesmo rig, outra roupa
    /// ou outro personagem). <see cref="Parts"/> mapeia o nome da parte (osso ou objeto com
    /// SpriteRenderer) para o sprite; <see cref="Remaps"/> troca sprites usados nas chaves de
    /// animação (ex.: "mao_fechada.png" da roupa A vira "mao_fechada_b.png" na roupa B), como os
    /// "character maps" do Spriter. Partes fora da skin ficam como estão.
    /// </summary>
    public sealed class Skin
    {
        public Skin(string name) => Name = name;

        public string Name { get; set; }

        /// <summary>Nome da parte (osso/objeto) → sprite.</summary>
        public Dictionary<string, SpriteRef> Parts { get; } = new();

        /// <summary>Chave do sprite original (<see cref="SpriteRef.Key"/> ou só o caminho) → sprite da skin.</summary>
        public Dictionary<string, SpriteRef> Remaps { get; } = new();

        /// <summary>Aplica o remapeamento da skin a um sprite de chave (ou devolve o original).</summary>
        public SpriteRef Remap(SpriteRef sprite)
        {
            if (Remaps.Count == 0)
                return sprite;
            if (Remaps.TryGetValue(sprite.Key, out var exact))
                return exact;
            if (sprite.TexturePath != null && Remaps.TryGetValue(sprite.TexturePath, out var byPath))
                return byPath.SourceRect == null ? byPath with { SourceRect = sprite.SourceRect } : byPath;
            return sprite;
        }

        public Skin Clone()
        {
            var copy = new Skin(Name);
            foreach (var (k, v) in Parts) copy.Parts[k] = v;
            foreach (var (k, v) in Remaps) copy.Remaps[k] = v;
            return copy;
        }
    }
}
