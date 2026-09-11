using DreamBit.Engine.Elements;
using DreamBit.Engine.Scripting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class ScriptTests
    {
        [TestMethod]
        public void Compile_ScriptValido_ExecutaOUpdate()
        {
            const string src =
                "public class S : IGameScript {" +
                "  public void Update(GameObject self, float dt) {" +
                "    self.Transform.Position += new Vector2(10f * dt, 0f);" +
                "  }" +
                "}";

            var (script, error) = ScriptCompiler.Compile(src);

            Assert.IsNull(error, error);
            Assert.IsNotNull(script);

            var obj = new GameObject();
            script!.Update(obj, 2f);
            Assert.AreEqual(20f, obj.Transform.Position.X, 0.01f);
        }

        [TestMethod]
        public void Compile_ScriptInvalido_RetornaErro()
        {
            var (script, error) = ScriptCompiler.Compile("public class Bad { isso nao compila ");

            Assert.IsNull(script);
            Assert.IsFalse(string.IsNullOrEmpty(error));
        }
    }
}
