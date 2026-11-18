using System.Collections.Generic;

namespace DreamBit.Engine.Serialization
{
    // DTOs da animação de esqueleto avançada (curvas, extras por osso, skins). Todos os campos
    // são opcionais: cenas antigas, sem eles, continuam carregando como antes.

    /// <summary>Curva de um segmento (ver Animation.KeyCurve).</summary>
    public sealed class KeyCurveData
    {
        public int Type { get; set; }
        public int Easing { get; set; }
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; } = 1f;
        public float Y2 { get; set; } = 1f;
    }

    /// <summary>Sprite (caminho + recorte opcional; W/H = 0 = textura inteira).</summary>
    public sealed class SpriteRefData
    {
        public string? Path { get; set; }
        public int SrcX { get; set; }
        public int SrcY { get; set; }
        public int SrcW { get; set; }
        public int SrcH { get; set; }
    }

    /// <summary>Extras de um osso num keyframe (ver Animation.BoneKey). Nulo = não definido.</summary>
    public sealed class BoneKeyData
    {
        public string Bone { get; set; } = "";
        public KeyCurveData? Curve { get; set; }
        public SpriteRefData? Sprite { get; set; }
        public int? SortOrder { get; set; }
        /// <summary>Cor "#RRGGBBAA".</summary>
        public string? Tint { get; set; }
        public bool? Visible { get; set; }
    }

    /// <summary>Uma entrada de skin: parte (ou chave de sprite a remapear) → sprite.</summary>
    public sealed class SkinEntryData
    {
        public string Key { get; set; } = "";
        public SpriteRefData Sprite { get; set; } = new();
    }

    /// <summary>Valor original de uma parte sobreposta por uma chave no momento de salvar (para
    /// a cena não perder o original quando o playhead está parado numa chave). Nulo = não sobreposto.</summary>
    public sealed class PartOriginalData
    {
        public string Part { get; set; } = "";
        public SpriteRefData? Sprite { get; set; }
        public int? SortOrder { get; set; }
        public string? Tint { get; set; }
        public bool? Enabled { get; set; }
    }

    public sealed class SkinData
    {
        public string Name { get; set; } = "skin";
        public List<SkinEntryData> Parts { get; set; } = new();
        public List<SkinEntryData>? Remaps { get; set; }
    }
}
