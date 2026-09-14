using System.Linq;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Xunit;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    public class BoneRigTests
    {
        [Fact]
        public void CapturaEResetaPoseDeDescanso()
        {
            var obj = new GameObject("Braço");
            obj.Transform.Position = new Vector2(10, 20);
            obj.Transform.Rotation = 0.5f;

            var bone = new Bone();
            obj.AddComponent(bone);
            bone.CaptureRestPose();

            // Posa o osso longe do descanso...
            obj.Transform.Position = new Vector2(200, 200);
            obj.Transform.Rotation = 2.0f;

            bone.ResetToRestPose();

            Assert.Equal(new Vector2(10, 20), obj.Transform.Position);
            Assert.Equal(0.5f, obj.Transform.Rotation, 0.0001f);
        }

        [Fact]
        public void SemPoseCapturada_ResetNaoFazNada()
        {
            var obj = new GameObject();
            obj.Transform.Position = new Vector2(5, 5);
            var bone = new Bone(); // sem CaptureRestPose
            obj.AddComponent(bone);

            bone.ResetToRestPose();

            Assert.Equal(new Vector2(5, 5), obj.Transform.Position);
            Assert.False(bone.HasRestPose);
        }

        [Fact]
        public void Hierarquia_RotacionarOssoPaiMoveAPontaDoFilho()
        {
            // Rig: pai na origem, filho a 40 de distância. Girar o pai 90° move a ponta do filho.
            var parent = new GameObject("Osso pai");
            parent.AddComponent(new Bone { Length = 40 });

            var child = new GameObject("Osso filho");
            var childBone = new Bone { Length = 40 };
            child.AddComponent(childBone);
            child.Transform.Position = new Vector2(40, 0); // preso na ponta do pai
            parent.AddChild(child);

            var tipAntes = childBone.WorldTip; // (80, 0) aprox
            parent.Transform.Rotation = MathHelper.PiOver2; // gira 90°

            var tipDepois = childBone.WorldTip;

            Assert.Equal(80f, tipAntes.X, 0.01f);
            Assert.Equal(0f, tipAntes.Y, 0.01f);
            // Após girar 90°, a ponta que estava em +X vai para +Y.
            Assert.Equal(0f, tipDepois.X, 0.01f);
            Assert.Equal(80f, tipDepois.Y, 0.01f);
        }

        [Fact]
        public void Serializacao_PreservaOssoEPose()
        {
            var scene = new Scene();
            var obj = new GameObject("Perna");
            obj.Transform.Position = new Vector2(7, 3);
            var bone = new Bone { Length = 55 };
            obj.AddComponent(bone);
            bone.CaptureRestPose();
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));

            var restored = loaded.Objects.First().Components.OfType<Bone>().Single();
            Assert.Equal(55f, restored.Length, 0.001f);
            Assert.True(restored.HasRestPose);
            Assert.Equal(new Vector2(7, 3), restored.RestPosition);
        }
    }
}
