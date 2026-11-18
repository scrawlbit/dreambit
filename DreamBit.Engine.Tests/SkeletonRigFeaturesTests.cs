using System;
using System.Collections.Generic;
using System.Linq;
using DreamBit.Engine.Animation;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    /// <summary>IK de dois ossos, skins, pontos de fixação, API de reprodução e serialização.</summary>
    public class SkeletonRigFeaturesTests
    {
        private static GameTime Frame(double s) => new(TimeSpan.Zero, TimeSpan.FromSeconds(s));

        // ---- IK ----

        /// <summary>Perna: coxa (0,0) → canela (100,0) com 100 de comprimento; alvo na cena.</summary>
        private static (Scene scene, GameObject root, GameObject coxa, GameObject canela, Bone canelaBone, GameObject alvo, IkConstraint ik) MakeLeg(Vector2 target)
        {
            var scene = new Scene();
            var root = new GameObject("Rig");
            var coxa = new GameObject("coxa");
            coxa.AddComponent(new Bone { Length = 100f });
            root.AddChild(coxa);
            var canela = new GameObject("canela");
            var canelaBone = canela.AddComponent(new Bone { Length = 100f });
            canela.Transform.Position = new Vector2(100, 0);
            coxa.AddChild(canela);
            var ik = root.AddComponent(new IkConstraint { UpperBone = "coxa", LowerBone = "canela", Target = "alvo" });
            scene.Add(root);

            var alvo = new GameObject("alvo");
            alvo.Transform.Position = target;
            scene.Add(alvo);
            return (scene, root, coxa, canela, canelaBone, alvo, ik);
        }

        private static void AssertNear(Vector2 expected, Vector2 actual, float tol = 0.5f)
            => Assert.True(Vector2.Distance(expected, actual) <= tol, $"esperado {expected}, obtido {actual}");

        [Fact]
        public void Ik_PontaAlcancaOAlvo_DobraPositiva()
        {
            var (_, _, _, canela, bone, _, ik) = MakeLeg(new Vector2(100, 100));
            Assert.True(ik.Solve());
            AssertNear(new Vector2(100, 100), bone.WorldTip);
            AssertNear(new Vector2(100, 0), canela.Transform.WorldPosition); // joelho
        }

        [Fact]
        public void Ik_DobraNegativa_EspelhaOJoelho()
        {
            var (_, _, _, canela, bone, _, ik) = MakeLeg(new Vector2(100, 100));
            ik.BendPositive = false;
            ik.Solve();
            AssertNear(new Vector2(100, 100), bone.WorldTip);
            AssertNear(new Vector2(0, 100), canela.Transform.WorldPosition);
        }

        [Fact]
        public void Ik_ComprimentosDosOssosSaoPreservados()
        {
            var (_, _, coxa, canela, bone, _, ik) = MakeLeg(new Vector2(60, 120));
            ik.Solve();
            Assert.Equal(100f, Vector2.Distance(coxa.Transform.WorldPosition, canela.Transform.WorldPosition), 0.01f);
            Assert.Equal(100f, Vector2.Distance(canela.Transform.WorldPosition, bone.WorldTip), 0.01f);
            AssertNear(new Vector2(60, 120), bone.WorldTip);
        }

        [Fact]
        public void Ik_AlvoForaDeAlcance_EsticaEmDirecaoAoAlvo()
        {
            var (_, _, _, canela, bone, _, ik) = MakeLeg(new Vector2(0, 500));
            ik.Solve();
            AssertNear(new Vector2(0, 100), canela.Transform.WorldPosition);
            AssertNear(new Vector2(0, 200), bone.WorldTip);
        }

        [Fact]
        public void Ik_MixZero_NaoMexe()
        {
            var (_, _, coxa, _, bone, _, ik) = MakeLeg(new Vector2(100, 100));
            ik.Mix = 0f;
            ik.Solve();
            Assert.Equal(0f, coxa.Transform.Rotation, 0.0001f);
            AssertNear(new Vector2(200, 0), bone.WorldTip);
        }

        [Fact]
        public void Ik_RigEspelhado_AlcancaOAlvoEEspelhaADobra()
        {
            var (_, root, _, canela, bone, _, ik) = MakeLeg(new Vector2(-100, 100));
            root.Transform.Scale = new Vector2(-1, 1); // personagem virado para a esquerda
            ik.Solve();
            AssertNear(new Vector2(-100, 100), bone.WorldTip);
            AssertNear(new Vector2(-100, 0), canela.Transform.WorldPosition); // espelho do caso positivo
        }

        [Fact]
        public void Ik_AlvoPorCodigo_SobrepoeOObjeto()
        {
            var (_, _, _, _, bone, _, ik) = MakeLeg(new Vector2(100, 100));
            ik.SetTargetPosition(new Vector2(150, 0));
            ik.Solve();
            AssertNear(new Vector2(150, 0), bone.WorldTip);
        }

        [Fact]
        public void Ik_EAvaliadoDepoisDaPoseDoAnimador()
        {
            var (scene, root, coxa, _, bone, _, _) = MakeLeg(new Vector2(100, 100));
            var anim = root.AddComponent(new SkeletonAnimator { Duration = 1f });
            coxa.Transform.Rotation = 0f; anim.CaptureKeyframe(0f);
            coxa.Transform.Rotation = 0.3f; anim.CaptureKeyframe(1f);

            scene.StartPlay();
            scene.Update(Frame(0.5));
            AssertNear(new Vector2(100, 100), bone.WorldTip);
        }

        [Fact]
        public void Ik_SemAnimador_ResolveNoUpdate()
        {
            var (scene, _, _, _, bone, alvo, _) = MakeLeg(new Vector2(100, 100));
            scene.StartPlay();
            alvo.Transform.Position = new Vector2(120, 60);
            scene.Update(Frame(0.016));
            AssertNear(new Vector2(120, 60), bone.WorldTip);
        }

        // ---- skins ----

        private static (GameObject root, SkeletonAnimator anim, SpriteRenderer cabeca, SpriteRenderer mao) MakeSkinned()
        {
            var root = new GameObject("Heroi");
            var anim = root.AddComponent(new SkeletonAnimator { Duration = 1f });
            var cabeca = new GameObject("cabeca");
            cabeca.AddComponent(new Bone());
            var sCabeca = cabeca.AddComponent(new SpriteRenderer { TexturePath = "lothar_cabeca.png" });
            root.AddChild(cabeca);
            var maoBone = new GameObject("mao");
            maoBone.AddComponent(new Bone());
            root.AddChild(maoBone);
            var maoArte = new GameObject("mao_arte");
            var sMao = maoArte.AddComponent(new SpriteRenderer { TexturePath = "lothar_mao.png" });
            maoBone.AddChild(maoArte);
            return (root, anim, sCabeca, sMao);
        }

        [Fact]
        public void Skin_CapturaETrocaTodasAsPartes()
        {
            var (_, anim, cabeca, mao) = MakeSkinned();
            var lothar = anim.CaptureSkin("lothar");
            Assert.Equal("lothar_cabeca.png", lothar.Parts["cabeca"].TexturePath);
            Assert.Equal("lothar_mao.png", lothar.Parts["mao"].TexturePath);

            var aria = anim.AddSkin("aria");
            aria.Parts["cabeca"] = new SpriteRef("aria.png", new Rectangle(0, 0, 32, 32));
            aria.Parts["mao"] = new SpriteRef("aria.png", new Rectangle(32, 0, 16, 16));

            anim.Skin = "aria";
            Assert.Equal("aria.png", cabeca.TexturePath);
            Assert.Equal(new Rectangle(0, 0, 32, 32), cabeca.SourceRect);
            Assert.Equal(new Rectangle(32, 0, 16, 16), mao.SourceRect);

            anim.Skin = "lothar";
            Assert.Equal("lothar_cabeca.png", cabeca.TexturePath);
            Assert.Null(cabeca.SourceRect);
            Assert.Equal("lothar_mao.png", mao.TexturePath);
        }

        [Fact]
        public void Skin_RemapeiaSpritesDasChaves_ERestauraParaOSpriteDaSkin()
        {
            var (_, anim, _, mao) = MakeSkinned();
            var k0 = anim.CaptureKeyframe(0f);
            anim.CaptureKeyframe(1f);
            var k = anim.CaptureKeyframe(0.5f);
            k.GetOrAddBoneKey("mao").Sprite = new SpriteRef("mao_fechada.png");

            var aria = anim.AddSkin("aria");
            aria.Parts["mao"] = new SpriteRef("aria_mao.png");
            aria.Remaps["mao_fechada.png"] = new SpriteRef("aria_mao_fechada.png");

            anim.Skin = "aria";
            anim.Sample(0.75f);
            Assert.Equal("aria_mao_fechada.png", mao.TexturePath);

            anim.Sample(0.25f);
            Assert.Equal("aria_mao.png", mao.TexturePath); // original agora é o da skin
            Assert.Empty(k0.BoneKeys);
        }

        [Fact]
        public void Skin_TrocadaDuranteChaveAtiva_ViraOOriginal()
        {
            var (_, anim, _, mao) = MakeSkinned();
            var k0 = anim.CaptureKeyframe(0f);
            anim.CaptureKeyframe(1f);
            k0.GetOrAddBoneKey("mao").Sprite = new SpriteRef("punho.png");
            anim.Sample(0.5f);
            Assert.Equal("punho.png", mao.TexturePath);

            anim.AddSkin("aria").Parts["mao"] = new SpriteRef("aria_mao.png");
            anim.Skin = "aria";
            Assert.Equal("punho.png", mao.TexturePath); // chave continua valendo
            anim.RestoreParts();
            Assert.Equal("aria_mao.png", mao.TexturePath);
        }

        // ---- pontos de fixação ----

        [Fact]
        public void AttachPoint_PosicaoERotacaoDeMundo()
        {
            var root = new GameObject("Heroi");
            root.Transform.Position = new Vector2(100, 50);
            var anim = root.AddComponent(new SkeletonAnimator());
            var braco = new GameObject("braco");
            braco.AddComponent(new Bone { Length = 40 });
            braco.Transform.Rotation = MathHelper.PiOver2; // aponta para baixo (Y+)
            root.AddChild(braco);
            braco.AddComponent(new AttachPoint { PointName = "ponta_espada", Offset = new Vector2(40, 0), Angle = 0f });

            Assert.True(anim.TryGetAttachPoint("ponta_espada", out var pos, out var rot));
            AssertNear(new Vector2(100, 90), pos, 0.01f);
            Assert.Equal(MathHelper.PiOver2, rot, 0.001f);
        }

        [Fact]
        public void AttachPoint_EspelhadoEPorNomeDeObjeto()
        {
            var root = new GameObject("Heroi");
            root.Transform.Scale = new Vector2(-1, 1);
            var anim = root.AddComponent(new SkeletonAnimator());
            var braco = new GameObject("braco");
            braco.AddComponent(new Bone());
            root.AddChild(braco);
            var mao = new GameObject("mao_d");
            mao.Transform.Position = new Vector2(30, 0);
            braco.AddChild(mao);
            mao.AddComponent(new AttachPoint { Offset = new Vector2(10, 0) }); // nome vazio = nome do objeto

            Assert.True(anim.TryGetAttachPoint("mao_d", out var pos, out var rot));
            AssertNear(new Vector2(-40, 0), pos, 0.01f);
            Assert.Equal(MathHelper.Pi, Math.Abs(rot), 0.001f); // virado para a esquerda

            Assert.True(anim.TryGetAttachPoint("braco", out var bpos, out _)); // sem componente: objeto
            AssertNear(Vector2.Zero, bpos, 0.01f);
            Assert.False(anim.TryGetAttachPoint("nao_existe", out _, out _));
        }

        // ---- reprodução ----

        private static (Scene scene, GameObject bone, SkeletonAnimator anim) MakePlayback()
        {
            var scene = new Scene();
            var root = new GameObject("Heroi");
            var anim = root.AddComponent(new SkeletonAnimator());
            var bone = new GameObject("b");
            bone.AddComponent(new Bone());
            root.AddChild(bone);
            scene.Add(root);

            anim.CurrentClip.Name = "ataque"; anim.Duration = 1f; anim.Loop = false;
            bone.Transform.Position = Vector2.Zero; anim.CaptureKeyframe(0f);
            bone.Transform.Position = new Vector2(100, 0); anim.CaptureKeyframe(1f);

            anim.AddClip("idle"); anim.Duration = 1f; anim.Loop = true;
            bone.Transform.Position = new Vector2(-50, 0); anim.CaptureKeyframe(0f);
            anim.CaptureKeyframe(1f);
            anim.CurrentClipName = "ataque";
            return (scene, bone, anim);
        }

        [Fact]
        public void Velocidade_EscalaOTempo()
        {
            var (scene, _, anim) = MakePlayback();
            scene.StartPlay();
            anim.Speed = 0.5f;
            scene.Update(Frame(0.5));
            Assert.Equal(0.25f, anim.Time, 0.001f);
            Assert.Equal(0.25f, anim.NormalizedTime, 0.001f);
        }

        [Fact]
        public void NormalizedTime_DefineEAplicaAPose()
        {
            var (_, bone, anim) = MakePlayback();
            anim.NormalizedTime = 0.5f;
            Assert.Equal(50f, bone.Transform.Position.X, 0.01f);
        }

        [Fact]
        public void ClipeSemLoop_Termina_EFilaTocaOProximo()
        {
            var (scene, bone, anim) = MakePlayback();
            var completos = new List<string>();
            anim.ClipCompleted += completos.Add;
            scene.StartPlay();

            anim.Queue("idle");
            Assert.Equal(new[] { "idle" }, anim.QueuedClips.ToArray());
            for (int i = 0; i < 12; i++) scene.Update(Frame(0.1));

            Assert.Equal("idle", anim.CurrentClipName);
            Assert.True(anim.IsPlaying);
            Assert.False(anim.IsFinished);
            Assert.Equal(-50f, bone.Transform.Position.X, 0.01f);
            Assert.Contains("ataque", completos);
        }

        [Fact]
        public void ClipeSemLoop_SemFila_FicaFinished()
        {
            var (scene, bone, anim) = MakePlayback();
            scene.StartPlay();
            Assert.False(anim.IsFinished);
            for (int i = 0; i < 12; i++) scene.Update(Frame(0.1));
            Assert.True(anim.IsFinished);
            Assert.False(anim.IsPlaying);
            Assert.Equal(100f, bone.Transform.Position.X, 0.01f);

            anim.Play("ataque");
            Assert.False(anim.IsFinished, "tocar de novo reinicia");
        }

        [Fact]
        public void Play_SemReiniciar_NaoVoltaAoInicio()
        {
            var (scene, _, anim) = MakePlayback();
            scene.StartPlay();
            scene.Update(Frame(0.3));
            anim.Play("ataque", 0f, restart: false);
            Assert.Equal(0.3f, anim.Time, 0.001f);
            anim.Play("ataque", 0f, restart: true);
            Assert.Equal(0f, anim.Time, 0.001f);
        }

        [Fact]
        public void Eventos_HandlerPorNome_EEventoNoTempoZero()
        {
            var (scene, _, anim) = MakePlayback();
            anim.SetEvents(new[] { (0f, "inicio"), (0.5f, "golpe") });
            int inicio = 0, golpe = 0;
            anim.OnEvent("inicio", () => inicio++);
            anim.OnEvent("golpe", () => golpe++);

            scene.StartPlay();
            for (int i = 0; i < 12; i++) scene.Update(Frame(0.1));
            Assert.Equal(1, inicio);
            Assert.Equal(1, golpe);
        }

        [Fact]
        public void Eventos_EmLoop_DisparamACadaCiclo()
        {
            var (scene, _, anim) = MakePlayback();
            anim.CurrentClipName = "idle";
            anim.SetEvents(new[] { (0f, "passo"), (0.5f, "passo") });
            int passos = 0;
            anim.AnimationEvent += n => { if (n == "passo") passos++; };

            scene.StartPlay();
            for (int i = 0; i < 23; i++) scene.Update(Frame(0.1)); // ~2.3 ciclos
            Assert.Equal(5, passos); // 0, 0.5, 1 (=0), 1.5, 2 (=0)
        }

        // ---- serialização ----

        [Fact]
        public void Serializacao_IdaEVolta_ComCurvasExtrasSkinsIkEAttach()
        {
            var scene = new Scene();
            var (root, anim, _, _) = MakeSkinned();
            var k0 = anim.CaptureKeyframe(0f);
            var k1 = anim.CaptureKeyframe(1f);
            k0.Curve = KeyCurve.Bezier(0.1f, 0.2f, 0.3f, 0.9f);
            var bk = k0.GetOrAddBoneKey("mao");
            bk.Curve = KeyCurve.Step;
            bk.Sprite = new SpriteRef("punho.png", new Rectangle(1, 2, 3, 4));
            bk.SortOrder = 7;
            bk.Tint = new Color(10, 20, 30, 40);
            bk.Visible = false;
            k1.GetOrAddBoneKey("cabeca").Curve = KeyCurve.FromEasing(Scrawlbit.EasingMode.Out);
            var aria = anim.AddSkin("aria");
            aria.Parts["cabeca"] = new SpriteRef("aria.png", new Rectangle(0, 0, 32, 32));
            aria.Remaps["punho.png"] = new SpriteRef("aria_punho.png");
            anim.Skin = "aria";
            anim.Speed = 1.5f;
            root.AddComponent(new IkConstraint { UpperBone = "cabeca", LowerBone = "mao", Target = "alvo", BendPositive = false, Mix = 0.25f });
            root.Children.First(c => c.Name == "mao").AddComponent(new AttachPoint { PointName = "mao_d", Offset = new Vector2(5, 6), Angle = 30f });
            scene.Add(root);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var lroot = loaded.Objects.First();
            var a = lroot.Components.OfType<SkeletonAnimator>().Single();

            Assert.Equal(1.5f, a.Speed);
            Assert.Equal("aria", a.Skin);
            Assert.Equal(KeyCurve.Bezier(0.1f, 0.2f, 0.3f, 0.9f), a.Keyframes[0].Curve);
            var lbk = a.Keyframes[0].GetBoneKey("mao")!;
            Assert.Equal(KeyCurve.Step, lbk.Curve);
            Assert.Equal(new SpriteRef("punho.png", new Rectangle(1, 2, 3, 4)), lbk.Sprite);
            Assert.Equal(7, lbk.SortOrder);
            Assert.Equal(new Color(10, 20, 30, 40), lbk.Tint);
            Assert.False(lbk.Visible);
            Assert.Equal(KeyCurve.FromEasing(Scrawlbit.EasingMode.Out), a.Keyframes[1].GetBoneKey("cabeca")!.Curve);
            var laria = a.GetSkin("aria")!;
            Assert.Equal(new SpriteRef("aria.png", new Rectangle(0, 0, 32, 32)), laria.Parts["cabeca"]);
            Assert.Equal(new SpriteRef("aria_punho.png"), laria.Remaps["punho.png"]);

            var ik = lroot.Components.OfType<IkConstraint>().Single();
            Assert.Equal("cabeca", ik.UpperBone);
            Assert.Equal("mao", ik.LowerBone);
            Assert.Equal("alvo", ik.Target);
            Assert.False(ik.BendPositive);
            Assert.Equal(0.25f, ik.Mix, 0.0001f);

            var ap = lroot.Children.First(c => c.Name == "mao").Components.OfType<AttachPoint>().Single();
            Assert.Equal("mao_d", ap.PointName);
            Assert.Equal(new Vector2(5, 6), ap.Offset);
            Assert.Equal(30f, ap.Angle, 0.0001f);
        }

        [Fact]
        public void Serializacao_CenaSemExtras_NaoGravaCamposNovos()
        {
            var scene = new Scene();
            var root = new GameObject("H");
            var anim = root.AddComponent(new SkeletonAnimator());
            var b = new GameObject("b"); b.AddComponent(new Bone()); root.AddChild(b);
            anim.CaptureKeyframe(0f);
            scene.Add(root);

            var json = SceneSerializer.SaveToString(scene);
            Assert.DoesNotContain("BoneKeys", json);
            Assert.DoesNotContain("Skins", json);
            Assert.DoesNotContain("\"Curve\"", json);
        }

        [Fact]
        public void Serializacao_LeJsonAntigoSemOsCamposNovos()
        {
            // Formato de antes desta versão: clipes e keyframes só com transforms.
            const string json = @"{""Name"":""Cena"",""Objects"":[{""Name"":""H"",
                ""Skeletons"":[{""CurrentClip"":""walk"",""Clips"":[
                  {""Name"":""walk"",""Duration"":2.0,""Loop"":true,""Easing"":3,
                   ""Keyframes"":[{""Time"":0.0,""Bones"":[{""Bone"":""b"",""Px"":0,""Py"":0,""Rot"":0,""Sx"":1,""Sy"":1}]},
                                  {""Time"":2.0,""Bones"":[{""Bone"":""b"",""Px"":10,""Py"":0,""Rot"":0,""Sx"":1,""Sy"":1}]}],
                   ""Events"":[{""Time"":1.0,""Name"":""passo""}]}]}],
                ""Children"":[{""Name"":""b"",""Bones"":[{""Length"":40}]}]}]}";

            var scene = SceneSerializer.LoadFromString(json);
            var root = scene.Objects.First();
            var a = root.Components.OfType<SkeletonAnimator>().Single();
            Assert.Equal("walk", a.CurrentClipName);
            Assert.Equal(1f, a.Speed);
            Assert.Equal("", a.Skin);
            Assert.Empty(a.Skins);
            Assert.Equal(2, a.KeyframeCount);
            Assert.Null(a.Keyframes[0].Curve);
            Assert.Empty(a.Keyframes[0].BoneKeys);
            Assert.Equal(Scrawlbit.EasingMode.InOut, a.Easing);

            a.Sample(1f);
            Assert.Equal(5f, root.Children[0].Transform.Position.X, 0.01f); // smoothstep(0.5) = 0.5
        }
    }
}
