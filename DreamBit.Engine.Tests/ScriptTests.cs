using DreamBit.Engine.Elements;
using DreamBit.Engine.Scripting;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class ScriptTests
    {
        [Fact]
        public void Compile_ScriptValido_ExecutaOUpdate()
        {
            const string src =
                "public class S : IGameScript {" +
                "  public void Update(GameObject self, float dt) {" +
                "    self.Transform.Position += new Vector2(10f * dt, 0f);" +
                "  }" +
                "}";

            var (script, error) = ScriptCompiler.Compile(src);

            Assert.Null(error);
            Assert.NotNull(script);

            var obj = new GameObject();
            script!.Update(obj, 2f);
            Assert.Equal(20f, obj.Transform.Position.X, 0.01f);
        }

        [Fact]
        public void Compile_ScriptInvalido_RetornaErro()
        {
            var (script, error) = ScriptCompiler.Compile("public class Bad { isso nao compila ");

            Assert.Null(script);
            Assert.False(string.IsNullOrEmpty(error));
        }
    }
}
