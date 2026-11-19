using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DreamBit.Engine.Animation;
using DreamBit.Engine.Components;
using DreamBit.Engine.Editing;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Studio.ViewModels
{
    /// <summary>
    /// Parte do inspetor dedicada ao rig de esqueleto: chave do osso selecionado (curva, troca
    /// de sprite, ordem, cor/alpha, visibilidade), curva do keyframe, skins e onion skin. Toda
    /// edição de chave/skin passa pelo histórico (snapshot do SkeletonAnimator).
    /// </summary>
    public sealed partial class InspectorViewModel
    {
        private const string NoSkin = "(nenhuma)";

        /// <summary>Histórico do editor (undo/redo das edições do rig). Definido pelo EditorViewModel.</summary>
        public History? History { get; set; }

        /// <summary>Opções de curva: "Padrão" herda (do keyframe ou do clipe).</summary>
        public IReadOnlyList<string> CurveOptions { get; } = new[] { "Padrão", "Linear", "In", "Out", "InOut", "Step", "Bézier" };

        public IReadOnlyList<string> VisibilityOptions { get; } = new[] { "Não muda", "Visível", "Oculto" };

        // ---- undo por snapshot ----

        private readonly record struct PartState(SpriteRenderer Part, string? Texture, Rectangle? Source)
        {
            public void Apply() { Part.TexturePath = Texture; Part.SourceRect = Source; }
        }

        private static List<PartState> CaptureParts(SkeletonAnimator s)
        {
            var list = new List<PartState>();
            if (s.Owner == null)
                return list;
            Walk(s.Owner);
            return list;

            void Walk(GameObject obj)
            {
                foreach (var sprite in obj.Components.OfType<SpriteRenderer>())
                    list.Add(new PartState(sprite, sprite.TexturePath, sprite.SourceRect));
                foreach (var child in obj.Children)
                    Walk(child);
            }
        }

        /// <summary>Executa uma edição do animador e registra no histórico (antes/depois).</summary>
        private void EditSkeleton(SkeletonAnimator s, string name, Action mutate, bool includeParts = false)
        {
            var before = s.CreateSnapshot();
            var partsBefore = includeParts ? CaptureParts(s) : null;
            mutate();
            s.RefreshBoneKeys();
            var after = s.CreateSnapshot();
            var partsAfter = includeParts ? CaptureParts(s) : null;

            if (History != null)
                History.Push(new EditorAction(name,
                    doAction: () => Restore(s, after, partsAfter),
                    undoAction: () => Restore(s, before, partsBefore)));
            else
                Refresh();
        }

        private static void Restore(SkeletonAnimator s, SkeletonAnimator.Snapshot snapshot, List<PartState>? parts)
        {
            s.RestoreSnapshot(snapshot);
            if (parts != null)
                foreach (var p in parts)
                    p.Apply();
            s.RefreshBoneKeys();
        }

        // ---- curvas (texto <-> KeyCurve) ----

        private static string CurveToOption(KeyCurve? curve) => curve switch
        {
            null => "Padrão",
            { Type: KeyCurveType.Step } => "Step",
            { Type: KeyCurveType.Bezier } => "Bézier",
            { } c => c.Easing.ToString()
        };

        private static KeyCurve? OptionToCurve(string? option, KeyCurve? current) => option switch
        {
            "Linear" => KeyCurve.Linear,
            "In" => KeyCurve.FromEasing(Scrawlbit.EasingMode.In),
            "Out" => KeyCurve.FromEasing(Scrawlbit.EasingMode.Out),
            "InOut" => KeyCurve.FromEasing(Scrawlbit.EasingMode.InOut),
            "Step" => KeyCurve.Step,
            "Bézier" => current is { Type: KeyCurveType.Bezier } ? current : KeyCurve.Bezier(0.42f, 0f, 0.58f, 1f),
            _ => null
        };

        private static string FormatBezier(KeyCurve? curve)
            => curve is { Type: KeyCurveType.Bezier } c
                ? string.Join(", ", new[] { c.X1, c.Y1, c.X2, c.Y2 }.Select(v => v.ToString("0.###", CultureInfo.InvariantCulture)))
                : string.Empty;

        private static bool TryParseNumbers(string? text, int count, out float[] values)
        {
            values = Array.Empty<float>();
            var parts = (text ?? string.Empty).Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != count)
                return false;
            var list = new float[count];
            for (int i = 0; i < count; i++)
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out list[i]))
                    return false;
            values = list;
            return true;
        }

        private static string FormatRect(Rectangle? r) => r is { } v ? $"{v.X}, {v.Y}, {v.Width}, {v.Height}" : string.Empty;

        private static bool TryParseRect(string? text, out Rectangle? rect)
        {
            rect = null;
            if (string.IsNullOrWhiteSpace(text))
                return true; // vazio = textura inteira
            if (!TryParseNumbers(text, 4, out var v))
                return false;
            rect = v[2] > 0 && v[3] > 0 ? new Rectangle((int)v[0], (int)v[1], (int)v[2], (int)v[3]) : null;
            return true;
        }

        // ---- contexto: osso selecionado dentro de um rig ----

        private static SkeletonAnimator? FindSkeleton(GameObject? obj)
        {
            for (var o = obj; o != null; o = o.Parent)
            {
                var s = o.Components.OfType<SkeletonAnimator>().FirstOrDefault();
                if (s != null)
                    return s;
            }
            return null;
        }

        /// <summary>Animador do rig do osso selecionado (no próprio objeto ou num ancestral).</summary>
        private SkeletonAnimator? RigSkeleton => Bone != null ? FindSkeleton(_target) : null;
        private PoseKeyframe? RigKeyframe => RigSkeleton is { } s ? s.KeyframeAt(s.Time) : null;
        private string RigBoneName => _target?.Name ?? string.Empty;
        private BoneKey? RigBoneKey => RigKeyframe?.GetBoneKey(RigBoneName);
        private SpriteRenderer? RigPart => RigSkeleton?.FindPart(RigBoneName);

        public bool HasRigContext => RigSkeleton != null;
        public bool HasRigKeyframe => RigKeyframe != null;
        public bool HasRigPart => RigPart != null;

        public string RigKeyInfo
        {
            get
            {
                var s = RigSkeleton;
                if (s == null) return string.Empty;
                var k = RigKeyframe;
                return k != null
                    ? $"Chave em t = {k.Time:0.00}s (clipe \"{s.CurrentClipName}\")"
                    : $"Sem keyframe em t = {s.Time:0.00}s: vá até uma chave ou capture uma (clipe \"{s.CurrentClipName}\").";
            }
        }

        public float RigDuration => RigSkeleton?.Duration ?? 1f;

        /// <summary>Tempo do clipe do rig (scrub a partir do osso selecionado).</summary>
        public float RigTime
        {
            get => RigSkeleton?.Time ?? 0f;
            set { var s = RigSkeleton; if (s != null) { s.SetTime(value); RaiseRig(); } }
        }

        public void RigPreviousKey()
        {
            var s = RigSkeleton;
            var k = s?.Keyframes.LastOrDefault(f => f.Time < s.Time - 0.0005f);
            if (s != null && k != null) { s.SetTime(k.Time); RaiseAll(); }
        }

        public void RigNextKey()
        {
            var s = RigSkeleton;
            var k = s?.Keyframes.FirstOrDefault(f => f.Time > s.Time + 0.0005f);
            if (s != null && k != null) { s.SetTime(k.Time); RaiseAll(); }
        }

        /// <summary>Captura a pose do rig inteiro como keyframe no tempo atual (a partir do osso).</summary>
        public void RigAddKeyframe()
        {
            var s = RigSkeleton;
            if (s != null)
                EditSkeleton(s, "Adicionar keyframe", () => s.CaptureKeyframe(s.Time));
        }

        private void EditBoneKey(string name, Action<BoneKey> mutate)
        {
            var s = RigSkeleton;
            var k = RigKeyframe;
            if (s == null || k == null)
                return;
            var bone = RigBoneName;
            float time = k.Time;
            EditSkeleton(s, name, () =>
            {
                var key = s.KeyframeAt(time, 0.0001f);
                if (key == null) return;
                mutate(key.GetOrAddBoneKey(bone));
                key.PruneBoneKeys();
            });
        }

        // curva do osso
        public string BoneKeyCurve
        {
            get => CurveToOption(RigBoneKey?.Curve);
            set { if (value != BoneKeyCurve) EditBoneKey("Curva do osso", bk => bk.Curve = OptionToCurve(value, bk.Curve)); }
        }
        public bool BoneKeyIsBezier => RigBoneKey?.Curve is { Type: KeyCurveType.Bezier };
        public string BoneKeyBezier
        {
            get => FormatBezier(RigBoneKey?.Curve);
            set
            {
                if (TryParseNumbers(value, 4, out var v))
                    EditBoneKey("Bézier do osso", bk => bk.Curve = KeyCurve.Bezier(v[0], v[1], v[2], v[3]));
                else
                    OnPropertyChanged();
            }
        }

        // troca de sprite
        private SpriteRef CurrentPartSprite()
        {
            var part = RigPart;
            return part != null ? new SpriteRef(part.TexturePath, part.SourceRect) : new SpriteRef(null);
        }

        public bool BoneKeyHasSprite
        {
            get => RigBoneKey?.Sprite != null;
            set { if (value != BoneKeyHasSprite) EditBoneKey(value ? "Trocar sprite na chave" : "Remover troca de sprite", bk => bk.Sprite = value ? CurrentPartSprite() : null); }
        }

        public string BoneKeySpritePath
        {
            get => RigBoneKey?.Sprite?.TexturePath ?? string.Empty;
            set
            {
                if (value == BoneKeySpritePath) return;
                EditBoneKey("Sprite da chave", bk => bk.Sprite = new SpriteRef(string.IsNullOrWhiteSpace(value) ? null : value.Trim(), bk.Sprite?.SourceRect));
            }
        }

        public string BoneKeySpriteRect
        {
            get => FormatRect(RigBoneKey?.Sprite?.SourceRect);
            set
            {
                if (value == BoneKeySpriteRect) return;
                if (TryParseRect(value, out var rect))
                    EditBoneKey("Recorte da chave", bk => bk.Sprite = new SpriteRef(bk.Sprite?.TexturePath, rect));
                else
                    OnPropertyChanged();
            }
        }

        /// <summary>Grava na chave o sprite que a parte mostra agora (ex.: escolhido no atlas).</summary>
        public void BoneKeyUseCurrentSprite()
        {
            var sprite = CurrentPartSprite();
            EditBoneKey("Sprite da chave", bk => bk.Sprite = sprite);
        }

        /// <summary>Define a textura da troca de sprite (arquivo escolhido no editor).</summary>
        public void BoneKeySetSpriteFile(string path)
        {
            EditBoneKey("Sprite da chave", bk => bk.Sprite = new SpriteRef(path, null));
        }

        // ordem de desenho
        public bool BoneKeyHasSortOrder
        {
            get => RigBoneKey?.SortOrder != null;
            set
            {
                if (value == BoneKeyHasSortOrder) return;
                int current = RigPart?.Owner.SortOrder ?? 0;
                EditBoneKey("Ordem na chave", bk => bk.SortOrder = value ? current : null);
            }
        }

        public int BoneKeySortOrder
        {
            get => RigBoneKey?.SortOrder ?? RigPart?.Owner.SortOrder ?? 0;
            set { if (value != BoneKeySortOrder) EditBoneKey("Ordem na chave", bk => bk.SortOrder = value); }
        }

        // cor / alpha
        public bool BoneKeyHasTint
        {
            get => RigBoneKey?.Tint != null;
            set
            {
                if (value == BoneKeyHasTint) return;
                var current = RigPart?.Tint ?? Color.White;
                EditBoneKey("Cor na chave", bk => bk.Tint = value ? current : null);
            }
        }

        private Color BoneKeyTintValue => RigBoneKey?.Tint ?? Color.White;

        private void SetTintChannel(Func<Color, Color> change)
            => EditBoneKey("Cor na chave", bk => bk.Tint = change(bk.Tint ?? Color.White));

        private static byte B(int v) => (byte)Math.Clamp(v, 0, 255);

        public int BoneKeyTintR { get => BoneKeyTintValue.R; set { if (value != BoneKeyTintR) SetTintChannel(c => new Color(B(value), c.G, c.B, c.A)); } }
        public int BoneKeyTintG { get => BoneKeyTintValue.G; set { if (value != BoneKeyTintG) SetTintChannel(c => new Color(c.R, B(value), c.B, c.A)); } }
        public int BoneKeyTintB { get => BoneKeyTintValue.B; set { if (value != BoneKeyTintB) SetTintChannel(c => new Color(c.R, c.G, B(value), c.A)); } }
        public int BoneKeyTintA { get => BoneKeyTintValue.A; set { if (value != BoneKeyTintA) SetTintChannel(c => new Color(c.R, c.G, c.B, B(value))); } }

        // visibilidade
        public string BoneKeyVisibility
        {
            get => RigBoneKey?.Visible switch { true => "Visível", false => "Oculto", _ => "Não muda" };
            set
            {
                if (value == BoneKeyVisibility) return;
                bool? visible = value switch { "Visível" => true, "Oculto" => false, _ => null };
                EditBoneKey("Visibilidade na chave", bk => bk.Visible = visible);
            }
        }

        // ---- curva do keyframe (seção Skeleton, rig do objeto selecionado) ----

        private PoseKeyframe? SkeletonKeyframe => Skeleton is { } s ? s.KeyframeAt(s.Time) : null;

        public bool SkeletonHasKeyAtTime => SkeletonKeyframe != null;

        public string SkeletonKeyCurve
        {
            get => CurveToOption(SkeletonKeyframe?.Curve);
            set
            {
                var s = Skeleton;
                var k = SkeletonKeyframe;
                if (s == null || k == null || value == SkeletonKeyCurve) return;
                float time = k.Time;
                EditSkeleton(s, "Curva do keyframe", () =>
                {
                    var key = s.KeyframeAt(time, 0.0001f);
                    if (key != null) key.Curve = OptionToCurve(value, key.Curve);
                });
            }
        }

        public bool SkeletonKeyIsBezier => SkeletonKeyframe?.Curve is { Type: KeyCurveType.Bezier };

        public string SkeletonKeyBezier
        {
            get => FormatBezier(SkeletonKeyframe?.Curve);
            set
            {
                var s = Skeleton;
                var k = SkeletonKeyframe;
                if (s == null || k == null) return;
                if (!TryParseNumbers(value, 4, out var v)) { OnPropertyChanged(); return; }
                float time = k.Time;
                EditSkeleton(s, "Bézier do keyframe", () =>
                {
                    var key = s.KeyframeAt(time, 0.0001f);
                    if (key != null) key.Curve = KeyCurve.Bezier(v[0], v[1], v[2], v[3]);
                });
            }
        }

        /// <summary>Velocidade de reprodução do animador (1 = normal).</summary>
        public float SkeletonSpeed
        {
            get => Skeleton?.Speed ?? 1f;
            set { var s = Skeleton; if (s != null) s.Speed = value; }
        }

        // ---- skins ----

        public IEnumerable<string> SkinOptions =>
            new[] { NoSkin }.Concat(Skeleton?.SkinNames ?? Enumerable.Empty<string>()).ToList();

        /// <summary>Skin de preview (aplica nas partes; com undo).</summary>
        public string SkeletonSkin
        {
            get => string.IsNullOrEmpty(Skeleton?.Skin) ? NoSkin : Skeleton!.Skin;
            set
            {
                var s = Skeleton;
                if (s == null || value == null || value == SkeletonSkin) return;
                var name = value == NoSkin ? string.Empty : value;
                EditSkeleton(s, "Trocar skin", () => s.Skin = name, includeParts: true);
            }
        }

        private string _newSkinName = "roupa";
        public string NewSkinName
        {
            get => _newSkinName;
            set { _newSkinName = value; OnPropertyChanged(); }
        }

        /// <summary>Cria uma skin com o visual atual das partes e a torna ativa.</summary>
        public void AddSkeletonSkin()
        {
            var s = Skeleton;
            if (s == null) return;
            var name = string.IsNullOrWhiteSpace(_newSkinName) ? "skin" : _newSkinName.Trim();
            EditSkeleton(s, "Nova skin", () => { s.CaptureSkin(name); s.Skin = name; }, includeParts: true);
        }

        /// <summary>Regrava a skin ativa com o visual atual das partes.</summary>
        public void UpdateSkeletonSkin()
        {
            var s = Skeleton;
            if (s == null || string.IsNullOrEmpty(s.Skin)) return;
            var name = s.Skin;
            EditSkeleton(s, "Atualizar skin", () => s.CaptureSkin(name), includeParts: true);
        }

        public void RemoveSkeletonSkin()
        {
            var s = Skeleton;
            if (s == null || string.IsNullOrEmpty(s.Skin)) return;
            var name = s.Skin;
            EditSkeleton(s, "Remover skin", () => s.RemoveSkin(name), includeParts: true);
        }

        /// <summary>
        /// Skin ativa em texto: uma linha por parte "parte: caminho | x, y, w, h" (recorte
        /// opcional) e remapeamentos de sprites de chaves como "&gt; original.png: novo.png".
        /// </summary>
        public string SkeletonSkinText
        {
            get
            {
                var skin = Skeleton?.CurrentSkin;
                if (skin == null) return string.Empty;
                var lines = skin.Parts.Select(p => $"{p.Key}: {FormatSprite(p.Value)}")
                    .Concat(skin.Remaps.Select(p => $"> {p.Key}: {FormatSprite(p.Value)}"));
                return string.Join("\n", lines);
            }
            set
            {
                var s = Skeleton;
                var skin = s?.CurrentSkin;
                if (s == null || skin == null || value == SkeletonSkinText) return;

                var parts = new Dictionary<string, SpriteRef>();
                var remaps = new Dictionary<string, SpriteRef>();
                foreach (var raw in (value ?? string.Empty).Split('\n'))
                {
                    var line = raw.Trim();
                    bool remap = line.StartsWith(">");
                    if (remap) line = line[1..].Trim();
                    int sep = line.IndexOf(": ", StringComparison.Ordinal);
                    if (sep <= 0) continue;
                    var key = line[..sep].Trim();
                    if (key.Length == 0 || !TryParseSprite(line[(sep + 2)..], out var sprite)) continue;
                    (remap ? remaps : parts)[key] = sprite;
                }

                var name = skin.Name;
                EditSkeleton(s, "Editar skin", () =>
                {
                    var target = s.GetSkin(name);
                    if (target == null) return;
                    target.Parts.Clear();
                    foreach (var (k, v) in parts) target.Parts[k] = v;
                    target.Remaps.Clear();
                    foreach (var (k, v) in remaps) target.Remaps[k] = v;
                    s.ApplySkin();
                }, includeParts: true);
            }
        }

        private static string FormatSprite(SpriteRef s)
            => s.SourceRect is { } r ? $"{s.TexturePath} | {r.X}, {r.Y}, {r.Width}, {r.Height}" : s.TexturePath ?? string.Empty;

        private static bool TryParseSprite(string text, out SpriteRef sprite)
        {
            sprite = default;
            var bar = text.LastIndexOf('|');
            var path = (bar >= 0 ? text[..bar] : text).Trim();
            Rectangle? rect = null;
            if (bar >= 0 && !TryParseRect(text[(bar + 1)..], out rect))
                return false;
            sprite = new SpriteRef(path.Length == 0 ? null : path, rect);
            return true;
        }

        // ---- onion skin (visual do editor; não é salvo) ----

        private bool _onionSkin;
        private int _onionSkinCount = 1;

        /// <summary>Mostra fantasmas das poses dos keyframes anteriores/seguintes no SceneView.</summary>
        public bool OnionSkinEnabled
        {
            get => _onionSkin;
            set { _onionSkin = value; OnPropertyChanged(); }
        }

        /// <summary>Quantos keyframes antes e depois desenhar (1 a 5).</summary>
        public int OnionSkinCount
        {
            get => _onionSkinCount;
            set { _onionSkinCount = Math.Clamp(value, 1, 5); OnPropertyChanged(); }
        }

        /// <summary>Animador cujo onion skin deve ser desenhado: o do objeto selecionado ou de um ancestral.</summary>
        public SkeletonAnimator? OnionSkeleton => _onionSkin ? FindSkeleton(_target) : null;

        private void RaiseRig()
        {
            foreach (var name in new[]
            {
                nameof(HasRigContext), nameof(HasRigKeyframe), nameof(HasRigPart), nameof(RigKeyInfo),
                nameof(RigDuration), nameof(RigTime),
                nameof(BoneKeyCurve), nameof(BoneKeyIsBezier), nameof(BoneKeyBezier),
                nameof(BoneKeyHasSprite), nameof(BoneKeySpritePath), nameof(BoneKeySpriteRect),
                nameof(BoneKeyHasSortOrder), nameof(BoneKeySortOrder),
                nameof(BoneKeyHasTint), nameof(BoneKeyTintR), nameof(BoneKeyTintG), nameof(BoneKeyTintB), nameof(BoneKeyTintA),
                nameof(BoneKeyVisibility),
                nameof(SkeletonHasKeyAtTime), nameof(SkeletonKeyCurve), nameof(SkeletonKeyIsBezier), nameof(SkeletonKeyBezier),
                nameof(SkeletonSpeed), nameof(SkinOptions), nameof(SkeletonSkin), nameof(SkeletonSkinText),
                nameof(OnionSkinEnabled), nameof(OnionSkinCount)
            })
                OnPropertyChanged(name);
        }
    }
}
