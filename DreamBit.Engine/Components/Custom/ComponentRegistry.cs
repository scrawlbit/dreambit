using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DreamBit.Engine.Elements;

namespace DreamBit.Engine.Components.Custom
{
    /// <summary>Um tipo de componente customizado registrado.</summary>
    public sealed record ComponentTypeInfo(string Id, Type Type, string MenuPath)
    {
        /// <summary>Nome curto (último segmento do caminho de menu).</summary>
        public string Name => MenuPath.Contains('/') ? MenuPath[(MenuPath.LastIndexOf('/') + 1)..] : MenuPath;

        /// <summary>Categoria (caminho de menu sem o nome), ou vazio.</summary>
        public string Category => MenuPath.Contains('/') ? MenuPath[..MenuPath.LastIndexOf('/')] : string.Empty;

        public SceneComponent Create() => (SceneComponent)Activator.CreateInstance(Type)!;
    }

    /// <summary>
    /// Registro dos componentes customizados (do jogo) conhecidos pela engine: a serialização
    /// de cena e o editor consultam este registro. O jogo registra os seus tipos na
    /// inicialização (<see cref="RegisterAssembly"/>); o editor carrega as DLLs listadas no
    /// projeto (.dbproj) e faz o mesmo.
    /// </summary>
    public static class ComponentRegistry
    {
        private static readonly Dictionary<string, ComponentTypeInfo> _byId = new(StringComparer.Ordinal);
        private static readonly Dictionary<Type, ComponentTypeInfo> _byType = new();
        private static readonly object _gate = new();

        // Componentes da própria engine marcados com [GameComponent] (Breakable, Pushable…)
        // são registrados sempre, sem precisar de DLL.
        static ComponentRegistry() => RegisterEngineTypes();

        private static void RegisterEngineTypes() => RegisterAssembly(typeof(ComponentRegistry).Assembly);

        /// <summary>True se o tipo vem da própria engine (não do jogo).</summary>
        public static bool IsEngineType(ComponentTypeInfo info) => info.Type.Assembly == typeof(ComponentRegistry).Assembly;

        /// <summary>Disparado quando tipos são registrados (o editor atualiza o menu).</summary>
        public static event Action? Changed;

        /// <summary>Tipos registrados, ordenados pelo caminho de menu.</summary>
        public static IReadOnlyList<ComponentTypeInfo> Types
        {
            get { lock (_gate) return _byId.Values.OrderBy(t => t.MenuPath, StringComparer.CurrentCultureIgnoreCase).ToList(); }
        }

        /// <summary>Registra um tipo de componente. Requer construtor público sem parâmetros.</summary>
        public static ComponentTypeInfo Register(Type type, string? id = null, string? menuPath = null)
        {
            if (!typeof(SceneComponent).IsAssignableFrom(type) || type.IsAbstract)
                throw new ArgumentException($"{type.FullName} não é um SceneComponent concreto.", nameof(type));
            if (type.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException($"{type.FullName} precisa de um construtor público sem parâmetros.", nameof(type));

            var attr = type.GetCustomAttribute<GameComponentAttribute>();
            var info = new ComponentTypeInfo(
                id ?? attr?.Id ?? type.FullName ?? type.Name,
                type,
                menuPath ?? attr?.MenuPath ?? type.Name);

            lock (_gate)
            {
                // Recarregar a mesma DLL (ex.: editor após recompilar) substitui o tipo antigo.
                if (_byId.TryGetValue(info.Id, out var old))
                    _byType.Remove(old.Type);
                _byId[info.Id] = info;
                _byType[type] = info;
            }
            Changed?.Invoke();
            return info;
        }

        public static ComponentTypeInfo Register<T>(string? id = null, string? menuPath = null) where T : SceneComponent, new()
            => Register(typeof(T), id, menuPath);

        /// <summary>Registra todos os tipos marcados com <see cref="GameComponentAttribute"/> do assembly.
        /// Retorna quantos foram registrados.</summary>
        public static int RegisterAssembly(Assembly assembly)
        {
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray()!; }

            int n = 0;
            foreach (var type in types)
            {
                if (type.IsAbstract || type.GetCustomAttribute<GameComponentAttribute>() == null)
                    continue;
                Register(type);
                n++;
            }
            return n;
        }

        /// <summary>Carrega uma DLL de componentes de um caminho (editor/Player) e registra seus tipos.</summary>
        public static int RegisterAssemblyFile(string path)
        {
            var full = System.IO.Path.GetFullPath(path);
            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => !a.IsDynamic && string.Equals(a.Location, full, StringComparison.OrdinalIgnoreCase));
            return RegisterAssembly(loaded ?? Assembly.LoadFrom(full));
        }

        public static bool TryGet(string id, out ComponentTypeInfo info)
        {
            lock (_gate) return _byId.TryGetValue(id, out info!);
        }

        public static bool TryGet(Type type, out ComponentTypeInfo info)
        {
            lock (_gate) return _byType.TryGetValue(type, out info!);
        }

        /// <summary>True se o componente é de um tipo customizado registrado.</summary>
        public static bool IsCustom(SceneComponent component) => TryGet(component.GetType(), out _);

        /// <summary>Remove todos os registros (testes).</summary>
        public static void Clear()
        {
            lock (_gate) { _byId.Clear(); _byType.Clear(); }
            RegisterEngineTypes();
            Changed?.Invoke();
        }
    }
}
