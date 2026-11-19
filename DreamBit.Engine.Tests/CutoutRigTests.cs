using System.IO;
using System.Linq;
using DreamBit.Engine.Animation;
using DreamBit.Engine.Assets;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class CutoutRigTests
    {
        private static void Clips(RigPoser p)
        {
            p.Clip("idle", 1f, true);
            p.Key(0f, new() { ["upper_arm_front"] = -10 });
            p.Key(1f, new() { ["upper_arm_front"] = 10 });
            p.Clip("run", 0.5f, true);
            p.Key(0f, new() { ["thigh_front"] = -30 });
            p.Key(0.5f, new() { ["thigh_front"] = 30 });
        }

        [Fact]
        public void SemManifesto_UsaEsqueletoPadrao_EApoiaOsPes()
        {
            var root = new GameObject("Heroi");
            var anim = CutoutRig.Build(root, "nao/existe", 100, 90f, 1f, Clips, Color.Red);
            var bones = anim.RigBones();
            Assert.Contains("pelvis", bones.Keys);
            Assert.Contains("shin_front", bones.Keys);
            Assert.Equal(new[] { "idle", "run" }, anim.ClipNames.ToArray());

            // O ponto mais baixo das partes fica em y = 90 (pés no chão).
            float bottom = root.Children.SelectMany(Descendants).Where(o => o.Components.OfType<SpriteRenderer>().Any())
                .Max(o => o.Transform.WorldPosition.Y + o.Components.OfType<SpriteRenderer>().First().Size.Y / 2f);
            Assert.InRange(bottom, 85f, 95f);
        }

        [Fact]
        public void ComManifesto_PosicionaOssosNasJuntas()
        {
            var dir = Path.Combine(Path.GetTempPath(), "dbrig_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(dir, "boneco"));
            File.WriteAllText(Path.Combine(dir, "boneco", "parts.json"), @"{
              ""scale"": 1.0,
              ""parts"": {
                ""pelvis"": { ""file"": ""pelvis.png"", ""w"": 40, ""h"": 20, ""pivot"": [20, 10], ""joints"": { ""waist"": [20, 0] } },
                ""torso"":  { ""file"": ""torso.png"",  ""w"": 40, ""h"": 60, ""pivot"": [20, 60], ""joints"": { ""neck"": [20, 0] } }
              },
              ""skeleton"": [ [""pelvis"", null, ""pelvis"", null, 1], [""torso"", ""pelvis"", ""torso"", ""waist"", 2] ]
            }");
            AssetPaths.AddRoot(dir);
            try
            {
                var root = new GameObject("Boneco");
                var anim = CutoutRig.Build(root, "boneco", 0, 0f, 1f, p => { p.Clip("idle", 1f, true); p.Key(0f, new()); }, Color.White);
                var torso = anim.RigBones()["torso"];
                Assert.Equal(new Vector2(0, -10), torso.Transform.Position); // junta "waist" (20,0) - pivô (20,10)
                var art = torso.Children.Single();
                Assert.Equal(new Vector2(0, -30), art.Transform.Position);   // centro (20,30) - pivô (20,60)
                Assert.Equal("boneco/torso.png", art.Components.OfType<SpriteRenderer>().Single().TexturePath);
            }
            finally
            {
                AssetPaths.ClearRoots();
                Directory.Delete(dir, true);
            }
        }

        [Fact]
        public void CharacterAnimator_TrocaClipePeloEstado_EEspelha()
        {
            var scene = new Scene();
            var floor = new GameObject("Chao");
            floor.Transform.Position = new Vector2(0, 100);
            floor.AddComponent(new BoxCollider { Size = new Vector2(4000, 40) });
            scene.Add(floor);
            var root = new GameObject("Heroi");
            root.Transform.Position = new Vector2(0, 80 - 40);
            var pc = root.AddComponent(new PlatformerController { HalfHeight = 40, HalfWidth = 12 });
            var anim = CutoutRig.Build(root, "nao/existe", 0, 40f, 1f, Clips, Color.Red);
            root.AddComponent(new CharacterAnimator());
            var input = new Input.VirtualInput();
            pc.InputOverride = input;
            scene.Add(root);
            scene.StartPlay();
            var frame = new GameTime(System.TimeSpan.Zero, System.TimeSpan.FromSeconds(1.0 / 60));
            for (int i = 0; i < 10; i++) { Input.Input.Update(); scene.Update(frame); }
            Assert.Equal("idle", anim.CurrentClipName);

            input.SetAxes(-1f);
            for (int i = 0; i < 10; i++) { Input.Input.Update(); scene.Update(frame); }
            Assert.Equal("run", anim.CurrentClipName);
            Assert.True(root.Transform.Scale.X < 0f); // virado para a esquerda
        }

        private static System.Collections.Generic.IEnumerable<GameObject> Descendants(GameObject o)
        {
            yield return o;
            foreach (var c in o.Children)
                foreach (var d in Descendants(c))
                    yield return d;
        }
    }
}
