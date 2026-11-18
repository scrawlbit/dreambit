using System;
using System.Linq;
using DreamBit.Engine.Animation;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    /// <summary>Curvas por keyframe/osso (easing, step, bézier) e chaves discretas/cor das partes.</summary>
    public class SkeletonCurvesAndKeysTests
    {
        private static (GameObject root, GameObject a, GameObject b, SkeletonAnimator anim) MakeRig()
        {
            var root = new GameObject("Heroi");
            var anim = new SkeletonAnimator { Duration = 1f, Loop = false };
            root.AddComponent(anim);
            var a = new GameObject("BracoA");
            a.AddComponent(new Bone());
            root.AddChild(a);
            var b = new GameObject("BracoB");
            b.AddComponent(new Bone());
            root.AddChild(b);
            return (root, a, b, anim);
        }

        /// <summary>Dois keyframes: os dois ossos vão de X=0 (t=0) a X=100 (t=1).</summary>
        private static (GameObject a, GameObject b, SkeletonAnimator anim, PoseKeyframe k0, PoseKeyframe k1) MakeMoving()
        {
            var (_, a, b, anim) = MakeRig();
            a.Transform.Position = Vector2.Zero; b.Transform.Position = Vector2.Zero;
            var k0 = anim.CaptureKeyframe(0f);
            a.Transform.Position = new Vector2(100, 0); b.Transform.Position = new Vector2(100, 0);
            var k1 = anim.CaptureKeyframe(1f);
            return (a, b, anim, k0, k1);
        }

        // ---- KeyCurve ----

        [Fact]
        public void Curva_Step_SeguraAteOFim()
        {
            Assert.Equal(0f, KeyCurve.Step.Evaluate(0f));
            Assert.Equal(0f, KeyCurve.Step.Evaluate(0.99f));
            Assert.Equal(1f, KeyCurve.Step.Evaluate(1f));
        }

        [Fact]
        public void Curva_Bezier_Extremos_Linear_ESimetrica()
        {
            var linear = KeyCurve.Bezier(0f, 0f, 1f, 1f);
            foreach (var t in new[] { 0f, 0.1f, 0.33f, 0.5f, 0.8f, 1f })
                Assert.Equal(t, linear.Evaluate(t), 0.001f);

            var easeInOut = KeyCurve.Bezier(0.42f, 0f, 0.58f, 1f);
            Assert.Equal(0f, easeInOut.Evaluate(0f), 0.0001f);
            Assert.Equal(1f, easeInOut.Evaluate(1f), 0.0001f);
            Assert.Equal(0.5f, easeInOut.Evaluate(0.5f), 0.001f); // simétrica
            Assert.True(easeInOut.Evaluate(0.2f) < 0.2f, "começa devagar");
            Assert.True(easeInOut.Evaluate(0.8f) > 0.8f, "termina devagar");
        }

        [Fact]
        public void Curva_Bezier_ValorConhecido()
        {
            // CSS "ease" = cubic-bezier(0.25, 0.1, 0.25, 1): em x=0.5, y ~ 0.8024.
            var ease = KeyCurve.Bezier(0.25f, 0.1f, 0.25f, 1f);
            Assert.Equal(0.8024f, ease.Evaluate(0.5f), 0.002f);
        }

        [Fact]
        public void Curva_Easing_IgualAoScrawlbit()
        {
            foreach (var mode in Enum.GetValues<Scrawlbit.EasingMode>())
                Assert.Equal(Scrawlbit.Easing.Apply(mode, 0.3f), KeyCurve.FromEasing(mode).Evaluate(0.3f), 0.00001f);
        }

        // ---- curva por keyframe e por osso ----

        [Fact]
        public void CurvaDoKeyframe_Step_SobrepoeOEasingDoClipe()
        {
            var (a, b, anim, k0, _) = MakeMoving();
            k0.Curve = KeyCurve.Step;

            anim.Sample(0.5f);
            Assert.Equal(0f, a.Transform.Position.X, 0.01f);
            Assert.Equal(0f, b.Transform.Position.X, 0.01f);

            anim.Sample(1f);
            Assert.Equal(100f, a.Transform.Position.X, 0.01f);
        }

        [Fact]
        public void CurvaDoOsso_SobrepoeADoKeyframe()
        {
            var (a, b, anim, k0, _) = MakeMoving();
            k0.Curve = KeyCurve.Linear;
            k0.GetOrAddBoneKey("BracoB").Curve = KeyCurve.Step;

            anim.Sample(0.5f);
            Assert.Equal(50f, a.Transform.Position.X, 0.01f); // keyframe linear
            Assert.Equal(0f, b.Transform.Position.X, 0.01f);  // osso B segura
        }

        [Fact]
        public void CurvaDoOsso_BezierInterpolaEntreChaves()
        {
            var (a, _, anim, k0, _) = MakeMoving();
            var curve = KeyCurve.Bezier(0.25f, 0.1f, 0.25f, 1f);
            k0.GetOrAddBoneKey("BracoA").Curve = curve;

            anim.Sample(0.5f);
            Assert.Equal(curve.Evaluate(0.5f) * 100f, a.Transform.Position.X, 0.05f);
        }

        [Fact]
        public void SemCurvas_ComportamentoAntigoMantido()
        {
            var (a, _, anim, _, _) = MakeMoving();
            anim.Easing = Scrawlbit.EasingMode.InOut;
            anim.Sample(0.25f);
            Assert.Equal(15.625f, a.Transform.Position.X, 0.01f); // smoothstep(0.25) * 100
        }

        [Fact]
        public void CapturarDeNovo_PreservaCurvaEExtrasDaChave()
        {
            var (a, _, anim, k0, _) = MakeMoving();
            k0.Curve = KeyCurve.Step;
            k0.GetOrAddBoneKey("BracoA").SortOrder = 5;

            a.Transform.Position = new Vector2(10, 0);
            var again = anim.CaptureKeyframe(0f);
            Assert.Same(k0, again);
            Assert.Equal(KeyCurve.Step, again.Curve);
            Assert.Equal(5, again.GetBoneKey("BracoA")!.SortOrder);
            Assert.Equal(10f, again.Bones["BracoA"].Position.X, 0.01f);
        }

        // ---- troca de sprite / ordem / visibilidade (discretos) ----

        private static SpriteRenderer AddSprite(GameObject obj, string texture)
        {
            var s = new SpriteRenderer { TexturePath = texture };
            obj.AddComponent(s);
            return s;
        }

        [Fact]
        public void TrocaDeSprite_EDiscretaESeguraAteAProximaChave()
        {
            var (a, _, anim, _, _) = MakeMoving();
            var mao = AddSprite(a, "mao_aberta.png");
            var k = anim.CaptureKeyframe(0.5f);
            k.GetOrAddBoneKey("BracoA").Sprite = new SpriteRef("mao_fechada.png", new Rectangle(1, 2, 30, 40));

            anim.Sample(0.25f);
            Assert.Equal("mao_aberta.png", mao.TexturePath); // antes da chave: original

            anim.Sample(0.5f);
            Assert.Equal("mao_fechada.png", mao.TexturePath);
            Assert.Equal(new Rectangle(1, 2, 30, 40), mao.SourceRect);

            anim.Sample(0.9f);
            Assert.Equal("mao_fechada.png", mao.TexturePath); // segura até o fim

            anim.Sample(0.1f);
            Assert.Equal("mao_aberta.png", mao.TexturePath);  // volta ao original
            Assert.Null(mao.SourceRect);
        }

        [Fact]
        public void TrocaDeSprite_SoRecorte_MantemATextura()
        {
            var (a, _, anim, k0, _) = MakeMoving();
            var rosto = AddSprite(a, "atlas.png");
            k0.GetOrAddBoneKey("BracoA").Sprite = new SpriteRef(null, new Rectangle(64, 0, 64, 64));

            anim.Sample(0.2f);
            Assert.Equal("atlas.png", rosto.TexturePath);
            Assert.Equal(new Rectangle(64, 0, 64, 64), rosto.SourceRect);
        }

        [Fact]
        public void ParteNoFilhoNaoOsso_RecebeAsChaves()
        {
            var (a, _, anim, k0, _) = MakeMoving();
            var pintura = new GameObject("BracoA_arte");
            a.AddChild(pintura);
            var sprite = AddSprite(pintura, "braco.png");
            k0.GetOrAddBoneKey("BracoA").Sprite = new SpriteRef("braco_sujo.png");

            Assert.Same(sprite, anim.FindPart("BracoA"));
            anim.Sample(0.5f);
            Assert.Equal("braco_sujo.png", sprite.TexturePath);
        }

        [Fact]
        public void OrdemDeDesenho_EVisibilidade_SaoDiscretas()
        {
            var (a, _, anim, _, _) = MakeMoving();
            var sprite = AddSprite(a, "braco.png");
            a.SortOrder = 1;
            var k = anim.CaptureKeyframe(0.5f);
            var bk = k.GetOrAddBoneKey("BracoA");
            bk.SortOrder = 10;
            bk.Visible = false;

            anim.Sample(0.4f);
            Assert.Equal(1, a.SortOrder);
            Assert.True(sprite.Enabled);

            anim.Sample(0.6f);
            Assert.Equal(10, a.SortOrder);
            Assert.False(sprite.Enabled, "parte escondida, sem esconder os ossos filhos");
            Assert.True(a.IsVisible);

            anim.Sample(0f);
            Assert.Equal(1, a.SortOrder);
            Assert.True(sprite.Enabled);
        }

        [Fact]
        public void Visibilidade_UltimaChaveVale()
        {
            var (a, _, anim, k0, k1) = MakeMoving();
            var sprite = AddSprite(a, "x.png");
            k0.GetOrAddBoneKey("BracoA").Visible = false;
            k1.GetOrAddBoneKey("BracoA").Visible = true;

            anim.Sample(0.99f);
            Assert.False(sprite.Enabled);
            anim.Sample(1f);
            Assert.True(sprite.Enabled);
        }

        // ---- cor / alpha (interpolados) ----

        [Fact]
        public void Tint_EInterpoladoEntreChaves()
        {
            var (a, _, anim, k0, k1) = MakeMoving();
            var sprite = AddSprite(a, "x.png");
            k0.GetOrAddBoneKey("BracoA").Tint = new Color(255, 255, 255, 255);
            k1.GetOrAddBoneKey("BracoA").Tint = new Color(255, 0, 0, 0);

            anim.Sample(0.5f);
            Assert.Equal(255, sprite.Tint.R);
            Assert.InRange(sprite.Tint.G, 126, 129);
            Assert.InRange(sprite.Tint.A, 126, 129);

            anim.Sample(1f);
            Assert.Equal(new Color(255, 0, 0, 0), sprite.Tint);
        }

        [Fact]
        public void Tint_UsaACurvaDaChaveAnterior()
        {
            var (a, _, anim, k0, k1) = MakeMoving();
            var sprite = AddSprite(a, "x.png");
            var bk = k0.GetOrAddBoneKey("BracoA");
            bk.Tint = Color.White;
            bk.Curve = KeyCurve.Step;
            k1.GetOrAddBoneKey("BracoA").Tint = Color.Black;

            anim.Sample(0.7f);
            Assert.Equal(Color.White, sprite.Tint);
        }

        [Fact]
        public void TrocarDeClipe_RestauraAsPartesQueOutroClipeMexeu()
        {
            var (a, _, anim, k0, _) = MakeMoving();
            var sprite = AddSprite(a, "mao_aberta.png");
            var bk = k0.GetOrAddBoneKey("BracoA");
            bk.Sprite = new SpriteRef("mao_fechada.png");
            bk.Tint = new Color(10, 20, 30, 40);
            anim.CurrentClip.Name = "ataque";
            anim.Sample(0.5f);
            Assert.Equal("mao_fechada.png", sprite.TexturePath);

            // "idle" não tem chaves da mão: ela volta ao original.
            anim.AddClip("idle");
            anim.CaptureKeyframe(0f);
            anim.Play("idle");
            anim.Sample(0f);
            Assert.Equal("mao_aberta.png", sprite.TexturePath);
            Assert.Equal(Color.White, sprite.Tint);
        }

        [Fact]
        public void ResolveBoneKeys_NaoAplica()
        {
            var (a, _, anim, k0, _) = MakeMoving();
            var sprite = AddSprite(a, "a.png");
            k0.GetOrAddBoneKey("BracoA").Sprite = new SpriteRef("b.png");

            var keys = anim.ResolveBoneKeys(0.5f);
            Assert.Equal("b.png", keys["BracoA"].Sprite!.Value.TexturePath);
            Assert.Equal("a.png", sprite.TexturePath);
        }

        [Fact]
        public void SpriteRenderer_TintPadraoEBranco()
        {
            Assert.Equal(Color.White, new SpriteRenderer().Tint);
        }

        // ---- onion skin: matrizes de outra pose sem mexer no rig ----

        [Fact]
        public void ComputeWorldMatrices_NaoAlteraOsTransforms()
        {
            var (a, _, anim, _, _) = MakeMoving();
            a.Transform.Position = new Vector2(7, 7);

            var m = anim.ComputeWorldMatrices(1f);
            var posNaPose = Vector2.Transform(Vector2.Zero, m[a]);
            Assert.Equal(100f, posNaPose.X, 0.01f);
            Assert.Equal(new Vector2(7, 7), a.Transform.Position);
        }

        // ---- snapshot / cópia ----

        [Fact]
        public void Snapshot_RestauraKeyframesExtrasESkins()
        {
            var (_, _, anim, k0, _) = MakeMoving();
            k0.Curve = KeyCurve.Step;
            anim.AddSkin("roupaA").Parts["BracoA"] = new SpriteRef("a.png");
            var snap = anim.CreateSnapshot();

            k0.Curve = null;
            anim.RemoveKeyframeNear(1f);
            anim.RemoveSkin("roupaA");

            anim.RestoreSnapshot(snap);
            Assert.Equal(2, anim.KeyframeCount);
            Assert.Equal(KeyCurve.Step, anim.Keyframes[0].Curve);
            Assert.NotNull(anim.GetSkin("roupaA"));
        }

        [Fact]
        public void Clone_CopiaIndependente()
        {
            var (_, _, anim, k0, _) = MakeMoving();
            k0.GetOrAddBoneKey("BracoA").Tint = Color.Red;
            anim.Speed = 2f;

            var copy = anim.Clone();
            Assert.Equal(2f, copy.Speed);
            Assert.Equal(Color.Red, copy.Keyframes[0].GetBoneKey("BracoA")!.Tint);

            copy.Keyframes[0].GetBoneKey("BracoA")!.Tint = Color.Blue;
            Assert.Equal(Color.Red, k0.GetBoneKey("BracoA")!.Tint);
        }
    }
}
