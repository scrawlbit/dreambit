using System.Collections.Generic;
using System.IO;
using System.Linq;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.Xna.Framework;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public enum Elemento { Fogo, Vento, Sombra }

    public sealed class Ponto { public string Nome { get; set; } = ""; public float Valor { get; set; } }

    [GameComponent("Teste/Corrupção", Id = "teste.corrupcao")]
    public sealed class CorrupcaoTeste : SceneComponent
    {
        public override string DisplayName => "Corrupção";
        public float Total { get; set; } = 200f;
        public int Nivel { get; set; } = 1;
        public bool Ativa { get; set; } = true;
        public string Nome { get; set; } = "Reanna";
        public Elemento Elemento { get; set; } = Elemento.Fogo;
        public Vector2 Offset { get; set; }
        public Point Celula { get; set; }
        public Color Cor { get; set; } = Color.White;
        public List<string> Tags { get; set; } = new();
        public Ponto Ponto { get; set; } = new();
        [HideInInspector] public float Oculta { get; set; } = 5f;
        public float SomenteLeitura => 3f;
    }

    public class CustomComponentTests
    {
        public CustomComponentTests() => ComponentRegistry.Register<CorrupcaoTeste>();

        [Fact]
        public void Registro_UsaIdEMenuDoAtributo()
        {
            Assert.True(ComponentRegistry.TryGet("teste.corrupcao", out var info));
            Assert.Equal("Teste/Corrupção", info.MenuPath);
            Assert.Equal("Corrupção", info.Name);
            Assert.Equal("Teste", info.Category);
        }

        [Fact]
        public void Propriedades_IgnoramOcultasESomenteLeitura()
        {
            var names = CustomComponentSerializer.GetProperties(typeof(CorrupcaoTeste)).Select(p => p.Name).ToList();
            Assert.Contains("Total", names);
            Assert.DoesNotContain("Oculta", names);
            Assert.DoesNotContain("SomenteLeitura", names);
            Assert.DoesNotContain("Enabled", names);
            Assert.DoesNotContain("Owner", names);
        }

        [Fact]
        public void Cena_IdaEVolta_PreservaTodosOsTipos()
        {
            var scene = new Scene();
            var obj = new GameObject("Reanna");
            obj.AddComponent(new CorrupcaoTeste
            {
                Total = 220, Nivel = 3, Ativa = false, Nome = "R", Elemento = Elemento.Sombra,
                Offset = new Vector2(1.5f, -2), Celula = new Point(4, 5), Cor = new Color(10, 20, 30, 40),
                Tags = new List<string> { "a", "b" }, Ponto = new Ponto { Nome = "p", Valor = 9 },
                Enabled = false
            });
            scene.Add(obj);

            var loaded = SceneSerializer.LoadFromString(SceneSerializer.SaveToString(scene));
            var c = loaded.Objects[0].Components.OfType<CorrupcaoTeste>().Single();
            Assert.Equal(220, c.Total);
            Assert.Equal(3, c.Nivel);
            Assert.False(c.Ativa);
            Assert.Equal("R", c.Nome);
            Assert.Equal(Elemento.Sombra, c.Elemento);
            Assert.Equal(new Vector2(1.5f, -2), c.Offset);
            Assert.Equal(new Point(4, 5), c.Celula);
            Assert.Equal(new Color(10, 20, 30, 40), c.Cor);
            Assert.Equal(new[] { "a", "b" }, c.Tags);
            Assert.Equal(9, c.Ponto.Valor);
            Assert.False(c.Enabled);
        }

        [Fact]
        public void TipoDesconhecido_ViraMissing_EPreservaDadosAoSalvar()
        {
            var data = new CustomComponentData { Type = "jogo.nao.carregado" };
            data.Properties["Vida"] = System.Text.Json.JsonSerializer.SerializeToElement(42);
            var scene = new Scene();
            var obj = new GameObject("x");
            obj.AddComponent(CustomComponentSerializer.FromData(data));
            scene.Add(obj);

            var json = SceneSerializer.SaveToString(scene);
            var again = SceneSerializer.LoadFromString(json);
            var missing = Assert.IsType<MissingComponent>(again.Objects[0].Components.Single());
            Assert.Equal("jogo.nao.carregado", missing.TypeId);
            Assert.Equal(42, missing.ToData().Properties["Vida"].GetInt32());
        }

        [Fact]
        public void Clone_CopiaValoresEmNovaInstancia()
        {
            var a = new CorrupcaoTeste { Total = 99 };
            var b = Assert.IsType<CorrupcaoTeste>(CustomComponentSerializer.Clone(a));
            Assert.NotSame(a, b);
            Assert.Equal(99, b.Total);
        }

        [Fact]
        public void Projeto_PersisteListaDeDlls()
        {
            var dir = Path.Combine(Path.GetTempPath(), "dbproj_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var p = Project.GameProject.CreateOrOpen(dir, "Jogo");
                p.ComponentAssemblies.Add("bin/Jogo.dll");
                p.Save();
                var again = Project.GameProject.CreateOrOpen(dir, "Jogo");
                Assert.Equal(new[] { "bin/Jogo.dll" }, again.ComponentAssemblies);
                var errors = again.LoadComponentAssemblies();
                Assert.Single(errors); // DLL inexistente vira erro, não exceção
            }
            finally { Directory.Delete(dir, true); }
        }
    }
}
