using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Animation
{
    /// <summary>
    /// Referência a um sprite pintado: caminho da textura + recorte opcional no atlas.
    /// <see cref="TexturePath"/> nulo/vazio mantém a textura atual da parte e troca só o recorte.
    /// </summary>
    public readonly record struct SpriteRef(string? TexturePath, Rectangle? SourceRect = null)
    {
        /// <summary>Chave textual ("caminho" ou "caminho@x,y,w,h") usada nos remapeamentos de skin.</summary>
        public string Key => SourceRect is { } r
            ? $"{TexturePath}@{r.X},{r.Y},{r.Width},{r.Height}"
            : TexturePath ?? string.Empty;
    }

    /// <summary>
    /// Dados extras de um osso num keyframe (além do transform em <see cref="BonePose"/>):
    /// curva própria do segmento, troca de sprite, ordem de desenho, cor/alpha e visibilidade
    /// da parte pintada do osso. Cada campo nulo significa "esta chave não mexe nisso".
    /// Sprite, ordem e visibilidade são discretos (valem da chave até a próxima que os defina);
    /// a cor é interpolada entre as chaves que a definem.
    /// </summary>
    public sealed class BoneKey
    {
        /// <summary>Curva do segmento que começa nesta chave, só para este osso (sobrepõe a do
        /// keyframe e a do clipe).</summary>
        public KeyCurve? Curve { get; set; }

        /// <summary>Troca de sprite da parte (mão aberta/fechada, expressões do rosto).</summary>
        public SpriteRef? Sprite { get; set; }

        /// <summary>Ordem de desenho (SortOrder) do objeto da parte.</summary>
        public int? SortOrder { get; set; }

        /// <summary>Cor multiplicada na parte (RGB + alpha, não pré-multiplicada).</summary>
        public Color? Tint { get; set; }

        /// <summary>Mostra/esconde a parte.</summary>
        public bool? Visible { get; set; }

        /// <summary>True se a chave não define nada (pode ser descartada).</summary>
        public bool IsEmpty => Curve == null && Sprite == null && SortOrder == null && Tint == null && Visible == null;

        public BoneKey Clone() => (BoneKey)MemberwiseClone();
    }

    /// <summary>Valores discretos/cor resolvidos de um osso num instante (ver
    /// <see cref="Components.SkeletonAnimator.ResolveBoneKeys"/>). Nulo = valor original da parte.</summary>
    public readonly record struct BoneKeySample(SpriteRef? Sprite, int? SortOrder, Color? Tint, bool? Visible)
    {
        public bool IsEmpty => Sprite == null && SortOrder == null && Tint == null && Visible == null;
    }
}
