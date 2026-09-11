using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DreamBit.Engine.Scripting
{
    /// <summary>
    /// Compila em runtime (Roslyn) um script C# do usuário que implementa
    /// <see cref="IGameScript"/>, com cache por código-fonte.
    /// </summary>
    public static class ScriptCompiler
    {
        private static readonly Dictionary<string, (IGameScript? Script, string? Error)> _cache = new();

        // Usings implícitos para o usuário não precisar declará-los.
        private const string Header =
            "using System;\n" +
            "using DreamBit.Engine.Elements;\n" +
            "using DreamBit.Engine.Scripting;\n" +
            "using Microsoft.Xna.Framework;\n";

        public static (IGameScript? Script, string? Error) Compile(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return (null, null);

            if (_cache.TryGetValue(source, out var cached))
                return cached;

            var result = CompileCore(source);
            _cache[source] = result;
            return result;
        }

        private static (IGameScript?, string?) CompileCore(string source)
        {
            try
            {
                var tree = CSharpSyntaxTree.ParseText(Header + source);

                var references = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))
                    .ToList();

                var compilation = CSharpCompilation.Create(
                    "Script_" + Guid.NewGuid().ToString("N"),
                    new[] { tree },
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using var stream = new MemoryStream();
                var emit = compilation.Emit(stream);
                if (!emit.Success)
                {
                    var errors = string.Join("\n", emit.Diagnostics
                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                        .Select(d => d.GetMessage()));
                    return (null, errors);
                }

                stream.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(stream.ToArray());

                var type = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(IGameScript).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
                if (type == null)
                    return (null, "Nenhuma classe implementa IGameScript.");

                return (Activator.CreateInstance(type) as IGameScript, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}
