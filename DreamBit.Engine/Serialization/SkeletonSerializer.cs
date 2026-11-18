using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Animation;
using DreamBit.Engine.Components;
using DreamBit.Engine.Components.Custom;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Serialization
{
    /// <summary>
    /// Conversão do <see cref="SkeletonAnimator"/> (clipes, keyframes, curvas, extras por osso e
    /// skins) de/para os DTOs da cena. Lê também o formato legado (single-clip) e cenas sem os
    /// campos novos.
    /// </summary>
    internal static class SkeletonSerializer
    {
        public static SkeletonData ToData(SkeletonAnimator skeleton)
        {
            var originals = skeleton.GetPartOriginals();
            return new SkeletonData
            {
                CurrentClip = skeleton.CurrentClipName,
                Clips = skeleton.Clips.Select(ToData).ToList(),
                Skin = string.IsNullOrEmpty(skeleton.Skin) ? null : skeleton.Skin,
                Speed = skeleton.Speed,
                Skins = skeleton.Skins.Count == 0 ? null : skeleton.Skins.Select(ToData).ToList(),
                PartOriginals = originals.Count == 0 ? null : originals.Select(o => new PartOriginalData
                {
                    Part = o.Part,
                    Sprite = ToData(o.Sprite),
                    SortOrder = o.SortOrder,
                    Tint = o.Tint is { } c ? CustomComponentSerializer.FormatColor(c) : null,
                    Enabled = o.Enabled
                }).ToList()
            };
        }

        private static PoseClipData ToData(PoseClip clip)
        {
            return new PoseClipData
            {
                Name = clip.Name,
                Duration = clip.Duration,
                Loop = clip.Loop,
                Easing = (int)clip.Easing,
                Events = clip.Events.Select(ev => new SkeletonEventData { Time = ev.Time, Name = ev.Name }).ToList(),
                Keyframes = clip.Keyframes.Select(k => new PoseKeyframeData
                {
                    Time = k.Time,
                    Bones = k.Bones.Select(b => new BonePoseData
                    {
                        Bone = b.Key,
                        Px = b.Value.Position.X, Py = b.Value.Position.Y,
                        Rot = b.Value.Rotation,
                        Sx = b.Value.Scale.X, Sy = b.Value.Scale.Y
                    }).ToList(),
                    Curve = ToData(k.Curve),
                    BoneKeys = k.BoneKeys.Count == 0 ? null : k.BoneKeys
                        .Where(bk => !bk.Value.IsEmpty)
                        .Select(bk => new BoneKeyData
                        {
                            Bone = bk.Key,
                            Curve = ToData(bk.Value.Curve),
                            Sprite = ToData(bk.Value.Sprite),
                            SortOrder = bk.Value.SortOrder,
                            Tint = bk.Value.Tint is { } c ? CustomComponentSerializer.FormatColor(c) : null,
                            Visible = bk.Value.Visible
                        }).ToList()
                }).ToList()
            };
        }

        private static SkinData ToData(Skin skin) => new()
        {
            Name = skin.Name,
            Parts = skin.Parts.Select(p => new SkinEntryData { Key = p.Key, Sprite = ToData(p.Value)! }).ToList(),
            Remaps = skin.Remaps.Count == 0 ? null
                : skin.Remaps.Select(p => new SkinEntryData { Key = p.Key, Sprite = ToData(p.Value)! }).ToList()
        };

        private static KeyCurveData? ToData(KeyCurve? curve) => curve is not { } c ? null : new KeyCurveData
        {
            Type = (int)c.Type, Easing = (int)c.Easing, X1 = c.X1, Y1 = c.Y1, X2 = c.X2, Y2 = c.Y2
        };

        private static SpriteRefData? ToData(SpriteRef? sprite)
        {
            if (sprite is not { } s)
                return null;
            var r = s.SourceRect ?? Rectangle.Empty;
            return new SpriteRefData { Path = s.TexturePath, SrcX = r.X, SrcY = r.Y, SrcW = r.Width, SrcH = r.Height };
        }

        public static SkeletonAnimator FromData(SkeletonData skel)
        {
            var animator = new SkeletonAnimator();

            // Formato novo (clipes) ou legado (single-clip) via um clipe "default".
            var clipDatas = skel.Clips.Count > 0
                ? skel.Clips
                : new List<PoseClipData>
                  {
                      new() { Name = "default", Duration = skel.Duration, Loop = skel.Loop,
                              Easing = skel.Easing, Keyframes = skel.Keyframes, Events = skel.Events }
                  };

            bool first = true;
            foreach (var clipData in clipDatas)
            {
                // O animador já nasce com "default"; renomeia-o no primeiro clipe.
                if (first) { animator.CurrentClip.Name = clipData.Name; first = false; }
                else animator.AddClip(clipData.Name);

                animator.Duration = clipData.Duration;
                animator.Loop = clipData.Loop;
                animator.Easing = (Scrawlbit.EasingMode)clipData.Easing;
                animator.SetEvents(clipData.Events.Select(ev => (ev.Time, ev.Name)));
                foreach (var kf in clipData.Keyframes)
                {
                    var bones = new Dictionary<string, BonePose>();
                    foreach (var b in kf.Bones)
                        bones[b.Bone] = new BonePose(new Vector2(b.Px, b.Py), b.Rot, new Vector2(b.Sx, b.Sy));
                    var frame = new PoseKeyframe(kf.Time, bones) { Curve = FromData(kf.Curve) };
                    if (kf.BoneKeys != null)
                        foreach (var bk in kf.BoneKeys)
                        {
                            if (string.IsNullOrEmpty(bk.Bone))
                                continue;
                            var key = new BoneKey
                            {
                                Curve = FromData(bk.Curve),
                                Sprite = FromData(bk.Sprite),
                                SortOrder = bk.SortOrder,
                                Tint = CustomComponentSerializer.ParseColor(bk.Tint),
                                Visible = bk.Visible
                            };
                            if (!key.IsEmpty)
                                frame.BoneKeys[bk.Bone] = key;
                        }
                    animator.AddKeyframe(frame);
                }
            }

            animator.CurrentClipName = skel.CurrentClip;

            if (skel.Skins != null)
                foreach (var sd in skel.Skins)
                {
                    var skin = animator.AddSkin(sd.Name);
                    foreach (var e in sd.Parts)
                        if (!string.IsNullOrEmpty(e.Key) && FromData(e.Sprite) is { } sp)
                            skin.Parts[e.Key] = sp;
                    if (sd.Remaps != null)
                        foreach (var e in sd.Remaps)
                            if (!string.IsNullOrEmpty(e.Key) && FromData(e.Sprite) is { } sp)
                                skin.Remaps[e.Key] = sp;
                }
            animator.Skin = skel.Skin ?? string.Empty; // ainda sem dono: só registra (a cena já tem os sprites)
            animator.Speed = skel.Speed;
            if (skel.PartOriginals != null)
                animator.SetPartOriginals(skel.PartOriginals
                    .Where(o => !string.IsNullOrEmpty(o.Part))
                    .Select(o => new SkeletonAnimator.PartOriginal(o.Part, FromData(o.Sprite), o.SortOrder,
                        CustomComponentSerializer.ParseColor(o.Tint), o.Enabled)));
            return animator;
        }

        private static KeyCurve? FromData(KeyCurveData? d)
        {
            if (d == null)
                return null;
            return (KeyCurveType)d.Type switch
            {
                KeyCurveType.Step => KeyCurve.Step,
                KeyCurveType.Bezier => KeyCurve.Bezier(d.X1, d.Y1, d.X2, d.Y2),
                _ => KeyCurve.FromEasing((Scrawlbit.EasingMode)d.Easing)
            };
        }

        private static SpriteRef? FromData(SpriteRefData? d)
        {
            if (d == null)
                return null;
            Rectangle? src = d.SrcW > 0 && d.SrcH > 0 ? new Rectangle(d.SrcX, d.SrcY, d.SrcW, d.SrcH) : null;
            return new SpriteRef(string.IsNullOrEmpty(d.Path) ? null : d.Path, src);
        }
    }
}
