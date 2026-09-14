using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests.Demo
{
    /// <summary>
    /// Monta um inimigo cutout (rig de <see cref="Bone"/> + <see cref="SkeletonAnimator"/>) usando
    /// as partes recortadas da personagem Fuse (Dust). Bones = hierarquia de Transform; cada osso
    /// tem uma parte (sprite) como filho. Cria clipes de pose idle/walk/attack/jump por keyframes.
    /// </summary>
    public static class FuseRig
    {
        private const string Parts = "DemoAssets/fuse-parts.png";
        private static List<Rectangle>? _rects;

        private static Rectangle Part(int i, string? dir)
        {
            if (_rects == null)
            {
                string path = Path.Combine(dir ?? ForestDemo.AssetsDir, "fuse-parts.json");
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                _rects = new List<Rectangle>();
                foreach (var p in doc.RootElement.EnumerateArray())
                    _rects.Add(new Rectangle(p.GetProperty("x").GetInt32(), p.GetProperty("y").GetInt32(),
                        p.GetProperty("w").GetInt32(), p.GetProperty("h").GetInt32()));
            }
            return _rects[i];
        }

        private static readonly Dictionary<string, Vector2> Rest = new();

        private static GameObject BoneObj(GameObject parent, string name, Vector2 local, float length = 24f)
        {
            var o = new GameObject(name);
            o.Transform.Position = local;
            o.AddComponent(new Bone { Length = length });
            Rest[name] = local;
            parent.AddChild(o);
            return o;
        }

        private static void AddPart(GameObject bone, int partIndex, Vector2 offset, string? dir, float scale = 1.5f)
        {
            var r = Part(partIndex, dir);
            var s = new GameObject(bone.Name + "_art");
            s.Transform.Position = offset;
            s.AddComponent(new SpriteRenderer
            {
                TexturePath = Parts,
                SourceRect = r,
                Size = new Vector2(r.Width * scale, r.Height * scale),
                Color = Color.White
            });
            bone.AddChild(s);
        }

        /// <summary>Cria o objeto inimigo com o rig completo e os clipes. Retorna a raiz.</summary>
        public static GameObject Build(Scene scene, Vector2 worldPos, string? assetsDir = null)
        {
            Rest.Clear();
            var root = new GameObject("Fuse") { Tag = "enemy" };
            root.Transform.Position = worldPos;
            var anim = new SkeletonAnimator();
            root.AddComponent(anim);
            scene.Add(root);

            // Hierarquia de ossos (local, Y para baixo). Figura de frente/lado, ~200px.
            var hip = BoneObj(root, "hip", Vector2.Zero);
            var torso = BoneObj(hip, "torso", new Vector2(0, -46));
            var head = BoneObj(torso, "head", new Vector2(4, -58));
            var armBack = BoneObj(torso, "armBack", new Vector2(-10, -46));
            var foreBack = BoneObj(armBack, "foreBack", new Vector2(0, 42));
            var armFront = BoneObj(torso, "armFront", new Vector2(12, -46));
            var foreFront = BoneObj(armFront, "foreFront", new Vector2(0, 42));
            var handFront = BoneObj(foreFront, "handFront", new Vector2(0, 34));
            var legBack = BoneObj(hip, "legBack", new Vector2(-10, -4));
            var legFront = BoneObj(hip, "legFront", new Vector2(10, -4));
            var tail = BoneObj(hip, "tail", new Vector2(-16, 6));

            // Partes (sprites) nos ossos.
            AddPart(torso, 13, new Vector2(0, -22), assetsDir);
            AddPart(head, 11, new Vector2(0, -12), assetsDir);
            AddPart(armBack, 4, new Vector2(0, 22), assetsDir);
            AddPart(foreBack, 9, new Vector2(0, 20), assetsDir);
            AddPart(armFront, 1, new Vector2(0, 22), assetsDir);
            AddPart(foreFront, 6, new Vector2(0, 18), assetsDir);
            AddPart(handFront, 12, new Vector2(0, 10), assetsDir);
            AddPart(legBack, 3, new Vector2(0, 44), assetsDir);
            AddPart(legFront, 0, new Vector2(0, 44), assetsDir);
            AddPart(tail, 16, new Vector2(-12, 12), assetsDir);

            BuildClips(anim);
            anim.CurrentClipName = "idle";
            return root;
        }

        // Aplica uma pose (rotações em graus por osso + deslocamento Y do quadril) e captura keyframe.
        private static void Pose(SkeletonAnimator anim, float time, Dictionary<string, float> deg, float hipDy = 0f)
        {
            foreach (var (name, obj) in anim.RigBones())
            {
                obj.Transform.Rotation = 0f;
                var rest = Rest[name];
                obj.Transform.Position = name == "hip" ? new Vector2(rest.X, rest.Y + hipDy) : rest;
            }
            foreach (var (name, d) in deg)
                if (anim.RigBones().TryGetValue(name, out var o))
                    o.Transform.Rotation = MathHelper.ToRadians(d);
            anim.CaptureKeyframe(time);
        }

        private static void BuildClips(SkeletonAnimator anim)
        {
            // idle: respiração/balanço sutil.
            anim.AddClip("idle"); anim.Duration = 1.6f; anim.Loop = true;
            Pose(anim, 0f, new() { ["armFront"] = 6, ["armBack"] = -6, ["tail"] = 8 }, hipDy: 0);
            Pose(anim, 0.8f, new() { ["torso"] = 2, ["head"] = -3, ["armFront"] = 10, ["armBack"] = -3, ["tail"] = -8 }, hipDy: 3);
            Pose(anim, 1.6f, new() { ["armFront"] = 6, ["armBack"] = -6, ["tail"] = 8 }, hipDy: 0);

            // walk: pernas e braços alternam.
            anim.AddClip("walk"); anim.Duration = 0.8f; anim.Loop = true;
            Pose(anim, 0f, new() { ["legFront"] = -28, ["legBack"] = 28, ["armFront"] = -22, ["armBack"] = 22, ["torso"] = 3 });
            Pose(anim, 0.4f, new() { ["legFront"] = 28, ["legBack"] = -28, ["armFront"] = 22, ["armBack"] = -22, ["torso"] = 3 });
            Pose(anim, 0.8f, new() { ["legFront"] = -28, ["legBack"] = 28, ["armFront"] = -22, ["armBack"] = 22, ["torso"] = 3 });

            // attack: braço da frente arma e desce (evento "hit" no impacto).
            anim.AddClip("attack"); anim.Duration = 0.5f; anim.Loop = false;
            Pose(anim, 0f, new() { ["armFront"] = -70, ["foreFront"] = -50, ["torso"] = -6 });
            Pose(anim, 0.22f, new() { ["armFront"] = 75, ["foreFront"] = 35, ["torso"] = 10, ["handFront"] = 20 });
            Pose(anim, 0.5f, new() { ["armFront"] = 6, ["foreFront"] = 0, ["torso"] = 0 });
            anim.SetEvents(new (float, string)[] { (0.22f, "hit") });

            // jump: encolhe as pernas e sobe os braços.
            anim.AddClip("jump"); anim.Duration = 0.6f; anim.Loop = false;
            Pose(anim, 0f, new() { ["legFront"] = 0, ["legBack"] = 0 }, hipDy: 0);
            Pose(anim, 0.3f, new() { ["legFront"] = -45, ["legBack"] = 45, ["armFront"] = -35, ["armBack"] = -35, ["head"] = -6 }, hipDy: -12);
            Pose(anim, 0.6f, new() { ["legFront"] = -20, ["legBack"] = 20, ["armFront"] = -10, ["armBack"] = -10 }, hipDy: -4);
        }
    }
}
