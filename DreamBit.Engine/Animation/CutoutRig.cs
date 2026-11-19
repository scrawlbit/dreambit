using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DreamBit.Engine.Assets;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Animation
{
    /// <summary>Manifesto das partes de um personagem cutout (parts.json): arquivo, tamanho, pivô e
    /// juntas de cada parte pintada, e o esqueleto (osso, pai, parte, junta no pai, ordem de desenho).</summary>
    public sealed class PartsManifest
    {
        public sealed class Part
        {
            public string File { get; set; } = "";
            public int W { get; set; }
            public int H { get; set; }
            public float[] Pivot { get; set; } = { 0, 0 };
            public Dictionary<string, float[]> Joints { get; set; } = new();

            public Vector2 PivotV => new(Pivot[0], Pivot[1]);
            public Vector2 Joint(string name) => Joints.TryGetValue(name, out var j) ? new Vector2(j[0], j[1]) : PivotV;
        }

        /// <summary>Linha do esqueleto: osso, pai, parte, junta no pai, ordem de desenho.</summary>
        public sealed record Row(string Bone, string? Parent, string Part, string? ParentJoint, int Order);

        public float Scale { get; set; } = 1f;
        public Dictionary<string, Part> Parts { get; set; } = new();
        public List<Row> Skeleton { get; } = new();

        public static PartsManifest? Load(string relativeDir)
        {
            var path = AssetPaths.Resolve(Path.Combine(relativeDir, "parts.json"));
            if (!File.Exists(path)) return null;
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            var m = new PartsManifest();
            if (root.TryGetProperty("scale", out var s)) m.Scale = s.GetSingle();
            foreach (var p in root.GetProperty("parts").EnumerateObject())
                m.Parts[p.Name] = JsonSerializer.Deserialize<Part>(p.Value.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            foreach (var row in root.GetProperty("skeleton").EnumerateArray())
            {
                var a = row.EnumerateArray().ToArray();
                string? Str(int i) => a.Length > i && a[i].ValueKind == JsonValueKind.String ? a[i].GetString() : null;
                m.Skeleton.Add(new Row(Str(0)!, Str(1), Str(2) ?? Str(0)!, Str(3), a.Length > 4 && a[4].ValueKind == JsonValueKind.Number ? a[4].GetInt32() : 0));
            }
            return m;
        }
    }

    /// <summary>
    /// Monta personagens cutout (ossos + partes pintadas) a partir de um <see cref="PartsManifest"/>
    /// e cria os clipes por poses-chave (<see cref="RigPoser"/>). Sem manifesto (arte ainda não
    /// feita), usa blocos coloridos com o esqueleto humanoide padrão — o jogo roda desde o primeiro dia.
    /// </summary>
    public static class CutoutRig
    {
        public const int CharacterLayer = 2;

        /// <summary>Ossos esperados de um personagem humanoide (com cauda).</summary>
        private static readonly PartsManifest.Row[] FallbackSkeleton =
        {
            new("pelvis", null, "pelvis", null, 5), new("torso", "pelvis", "torso", "waist", 6),
            new("head", "torso", "head", "neck", 8), new("tail_base", "pelvis", "tail_base", "tail", 1),
            new("tail_tip", "tail_base", "tail_tip", "tail_tip", 0),
            new("upper_arm_back", "torso", "upper_arm_back", "shoulder_back", 2), new("lower_arm_back", "upper_arm_back", "lower_arm_back", "elbow", 1),
            new("thigh_back", "pelvis", "thigh_back", "hip_back", 3), new("shin_back", "thigh_back", "shin_back", "knee", 2),
            new("thigh_front", "pelvis", "thigh_front", "hip_front", 7), new("shin_front", "thigh_front", "shin_front", "knee", 6),
            new("upper_arm_front", "torso", "upper_arm_front", "shoulder_front", 10), new("lower_arm_front", "upper_arm_front", "lower_arm_front", "elbow", 11),
        };

        /// <summary>Cria o rig como filhos de <paramref name="root"/>, com os pés em y = <paramref name="feetY"/>.
        /// Retorna o animador (já com os clipes de <paramref name="clips"/>).</summary>
        public static SkeletonAnimator Build(GameObject root, string artDir, int sortBase, float feetY, float scale,
            Action<RigPoser> clips, Color fallbackColor)
        {
            var manifest = PartsManifest.Load(artDir);
            var anim = root.AddComponent(new SkeletonAnimator());
            var bones = new Dictionary<string, GameObject>();
            var poser = new RigPoser(anim);
            float s = (manifest?.Scale ?? 1f) * scale;

            var rows = manifest?.Skeleton.Count > 0 ? manifest.Skeleton : FallbackSkeleton.ToList();
            foreach (var row in rows)
            {
                var parent = row.Parent != null && bones.TryGetValue(row.Parent, out var p) ? p : root;
                var bone = new GameObject(row.Bone);
                bone.AddComponent(new Bone { Length = 0f });

                Vector2 local = Vector2.Zero;
                if (manifest != null && row.Parent != null && bones.ContainsKey(row.Parent) &&
                    manifest.Parts.TryGetValue(rows.First(r => r.Bone == row.Parent).Part, out var parentPart))
                    local = (parentPart.Joint(row.ParentJoint ?? "") - parentPart.PivotV) * s;
                else if (manifest == null)
                    local = FallbackOffset(row.Bone) * scale;
                bone.Transform.Position = local;
                parent.AddChild(bone);
                bones[row.Bone] = bone;
                poser.Rest[row.Bone] = local;

                // Parte pintada como filha do osso, centralizada em relação ao pivô.
                var art = new GameObject(row.Bone + "_parte") { SortOrder = sortBase + row.Order, RenderLayer = CharacterLayer };
                if (manifest != null && manifest.Parts.TryGetValue(row.Part, out var part))
                {
                    var center = new Vector2(part.W, part.H) / 2f;
                    art.Transform.Position = (center - part.PivotV) * s;
                    art.AddComponent(new SpriteRenderer
                    {
                        TexturePath = Path.Combine(artDir, part.File).Replace('\\', '/'),
                        Size = new Vector2(part.W, part.H) * s
                    });
                }
                else
                {
                    var size = FallbackSize(row.Bone) * scale;
                    art.Transform.Position = new Vector2(0, size.Y / 2f) * (row.Bone is "torso" or "head" ? -1f : 1f);
                    art.AddComponent(new SpriteRenderer { Size = size, Color = Shade(fallbackColor, row.Bone) });
                }
                bone.AddChild(art);
            }

            // Pés no chão: desloca a pélvis para o ponto mais baixo das partes ficar em feetY.
            if (bones.TryGetValue("pelvis", out var pelvis))
            {
                float bottom = LowestPoint(root, bones.Values);
                pelvis.Transform.Position += new Vector2(0, feetY - bottom);
                poser.Rest["pelvis"] = pelvis.Transform.Position;
            }

            clips(poser);
            anim.CurrentClipName = anim.Clips.FirstOrDefault()?.Name ?? "default";
            poser.ApplyRest();
            return anim;
        }

        private static float LowestPoint(GameObject root, IEnumerable<GameObject> bones)
        {
            float bottom = float.MinValue;
            var inv = Matrix.Invert(root.Transform.WorldMatrix);
            foreach (var b in bones)
                foreach (var part in b.Children.Where(c => c.Components.OfType<SpriteRenderer>().Any()))
                {
                    var sr = part.Components.OfType<SpriteRenderer>().First();
                    var world = part.Transform.WorldMatrix;
                    foreach (var corner in new[] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f) })
                    {
                        var p = Vector2.Transform(Vector2.Transform(corner * sr.Size, world), inv);
                        bottom = Math.Max(bottom, p.Y);
                    }
                }
            return bottom == float.MinValue ? 0f : bottom;
        }

        private static Vector2 FallbackOffset(string bone) => bone switch
        {
            "torso" => new(0, -10), "head" => new(4, -70), "tail_base" => new(-14, 6), "tail_tip" => new(-30, 10),
            "upper_arm_back" => new(-6, -62), "lower_arm_back" => new(0, 40), "upper_arm_front" => new(8, -62), "lower_arm_front" => new(0, 40),
            "thigh_back" => new(-6, 4), "shin_back" => new(0, 44), "thigh_front" => new(6, 4), "shin_front" => new(0, 44),
            _ => Vector2.Zero
        };

        private static Vector2 FallbackSize(string bone) => bone switch
        {
            "torso" => new(40, 70), "head" => new(36, 40), "pelvis" => new(40, 24), "tail_base" => new(30, 14), "tail_tip" => new(28, 10),
            "upper_arm_back" or "upper_arm_front" => new(12, 42), "lower_arm_back" or "lower_arm_front" => new(10, 40),
            "thigh_back" or "thigh_front" => new(16, 46), "shin_back" or "shin_front" => new(14, 46),
            _ => new(10, 10)
        };

        private static Color Shade(Color c, string bone) => bone.Contains("back") || bone.StartsWith("tail")
            ? new Color(c.R * 3 / 4, c.G * 3 / 4, c.B * 3 / 4) : c;
    }

    /// <summary>Ajuda a criar clipes: posa ossos por rotação (graus) e desloca a pélvis, e captura chaves.</summary>
    public sealed class RigPoser
    {
        private readonly SkeletonAnimator _anim;

        public RigPoser(SkeletonAnimator anim) => _anim = anim;

        public Dictionary<string, Vector2> Rest { get; } = new();

        public void Clip(string name, float duration, bool loop)
        {
            // O animador nasce com um clipe "default" vazio: o primeiro clipe criado o reaproveita.
            if (_anim.GetClip(name) == null && _anim.Clips.Count == 1 && _anim.Clips[0].Name == "default" && _anim.Clips[0].Keyframes.Count == 0)
                _anim.Clips[0].Name = name;
            if (_anim.GetClip(name) == null) _anim.AddClip(name);
            _anim.CurrentClipName = name;
            _anim.Duration = duration;
            _anim.Loop = loop;
        }

        /// <summary>Pose em <paramref name="time"/>: rotações em graus (positivo = horário) e
        /// deslocamento da pélvis (px).</summary>
        public void Key(float time, Dictionary<string, float> degrees, float pelvisDx = 0f, float pelvisDy = 0f)
        {
            ApplyRest();
            var bones = _anim.RigBones();
            foreach (var (name, deg) in degrees)
                if (bones.TryGetValue(name, out var b))
                    b.Transform.Rotation = MathHelper.ToRadians(deg);
            if (bones.TryGetValue("pelvis", out var pelvis) && Rest.TryGetValue("pelvis", out var rp))
                pelvis.Transform.Position = rp + new Vector2(pelvisDx, pelvisDy);
            _anim.CaptureKeyframe(time);
        }

        public void Events(params (float Time, string Name)[] events) => _anim.SetEvents(events);

        public void ApplyRest()
        {
            foreach (var (name, b) in _anim.RigBones())
            {
                b.Transform.Rotation = 0f;
                if (Rest.TryGetValue(name, out var r)) b.Transform.Position = r;
            }
        }
    }
}
