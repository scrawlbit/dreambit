using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Serialization;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Components.Custom
{
    /// <summary>Uma propriedade editável/serializável de um componente customizado.</summary>
    public sealed record ComponentProperty(PropertyInfo Info, string? Tooltip, RangeAttribute? Range, AssetPathAttribute? AssetPath)
    {
        public string Name => Info.Name;
        public Type Type => Info.PropertyType;
        public object? Get(SceneComponent c) => Info.GetValue(c);
        public void Set(SceneComponent c, object? value) => Info.SetValue(c, value);
    }

    /// <summary>
    /// Serializa componentes customizados por reflexão: todas as propriedades públicas de
    /// instância com get e set públicos, exceto as da base (Owner/Enabled/DisplayName) e as
    /// marcadas com <see cref="HideInInspectorAttribute"/> ou <c>[JsonIgnore]</c>. Tipos
    /// suportados: primitivos, string, enum, Vector2, Point, Rectangle, Color, listas/arrays e
    /// classes simples (POCO) desses tipos.
    /// </summary>
    public static class CustomComponentSerializer
    {
        private static readonly Dictionary<Type, IReadOnlyList<ComponentProperty>> _props = new();

        /// <summary>Opções JSON com conversores de tipos do MonoGame.</summary>
        public static JsonSerializerOptions JsonOptions { get; } = CreateOptions();

        private static JsonSerializerOptions CreateOptions()
        {
            var o = new JsonSerializerOptions { WriteIndented = false };
            o.Converters.Add(new Vector2Converter());
            o.Converters.Add(new PointConverter());
            o.Converters.Add(new RectangleConverter());
            o.Converters.Add(new ColorConverter());
            o.Converters.Add(new JsonStringEnumConverter());
            return o;
        }

        /// <summary>Propriedades editáveis do tipo, na ordem de declaração.</summary>
        public static IReadOnlyList<ComponentProperty> GetProperties(Type type)
        {
            lock (_props)
            {
                if (_props.TryGetValue(type, out var cached))
                    return cached;

                var list = new List<ComponentProperty>();
                foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.MetadataToken))
                {
                    if (p.DeclaringType == typeof(SceneComponent) || p.DeclaringType == typeof(Notification.NotificationObject))
                        continue;
                    if (!p.CanRead || !p.CanWrite || p.GetSetMethod() == null || p.GetIndexParameters().Length > 0)
                        continue;
                    if (p.IsDefined(typeof(HideInInspectorAttribute)) || p.IsDefined(typeof(JsonIgnoreAttribute)))
                        continue;
                    list.Add(new ComponentProperty(p,
                        p.GetCustomAttribute<TooltipAttribute>()?.Text,
                        p.GetCustomAttribute<RangeAttribute>(),
                        p.GetCustomAttribute<AssetPathAttribute>()));
                }
                // Ordem estável: MetadataToken segue a declaração na classe; a base vem depois.
                return _props[type] = list;
            }
        }

        /// <summary>Componente → dados de cena.</summary>
        public static CustomComponentData ToData(SceneComponent component)
        {
            if (component is MissingComponent missing)
                return missing.ToData();

            if (!ComponentRegistry.TryGet(component.GetType(), out var info))
                throw new InvalidOperationException($"{component.GetType().FullName} não está registrado no ComponentRegistry.");

            var data = new CustomComponentData { Type = info.Id, Enabled = component.Enabled };
            foreach (var p in GetProperties(component.GetType()))
            {
                try { data.Properties[p.Name] = JsonSerializer.SerializeToElement(p.Get(component), p.Type, JsonOptions); }
                catch { /* propriedade não serializável: ignorada */ }
            }
            return data;
        }

        /// <summary>Dados de cena → componente. Tipo desconhecido vira <see cref="MissingComponent"/>
        /// (os dados são preservados e regravados intactos).</summary>
        public static SceneComponent FromData(CustomComponentData data)
        {
            if (!ComponentRegistry.TryGet(data.Type, out var info))
                return new MissingComponent(data);

            var component = info.Create();
            component.Enabled = data.Enabled;
            var props = GetProperties(info.Type).ToDictionary(p => p.Name, StringComparer.Ordinal);
            foreach (var (name, element) in data.Properties)
            {
                if (!props.TryGetValue(name, out var p))
                    continue; // propriedade removida/renomeada no código
                try { p.Set(component, element.Deserialize(p.Type, JsonOptions)); }
                catch { /* tipo mudou: mantém o valor padrão */ }
            }
            return component;
        }

        /// <summary>Copia um componente customizado (duplicar objeto, prefabs).</summary>
        public static SceneComponent Clone(SceneComponent component) => FromData(ToData(component));

        // ---- conversores ----

        private sealed class Vector2Converter : JsonConverter<Vector2>
        {
            public override Vector2 Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
            {
                var e = JsonElement.ParseValue(ref r);
                return new Vector2(Num(e, "X"), Num(e, "Y"));
            }
            public override void Write(Utf8JsonWriter w, Vector2 v, JsonSerializerOptions o)
            {
                w.WriteStartObject(); w.WriteNumber("X", v.X); w.WriteNumber("Y", v.Y); w.WriteEndObject();
            }
        }

        private sealed class PointConverter : JsonConverter<Point>
        {
            public override Point Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
            {
                var e = JsonElement.ParseValue(ref r);
                return new Point((int)Num(e, "X"), (int)Num(e, "Y"));
            }
            public override void Write(Utf8JsonWriter w, Point v, JsonSerializerOptions o)
            {
                w.WriteStartObject(); w.WriteNumber("X", v.X); w.WriteNumber("Y", v.Y); w.WriteEndObject();
            }
        }

        private sealed class RectangleConverter : JsonConverter<Rectangle>
        {
            public override Rectangle Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
            {
                var e = JsonElement.ParseValue(ref r);
                return new Rectangle((int)Num(e, "X"), (int)Num(e, "Y"), (int)Num(e, "Width"), (int)Num(e, "Height"));
            }
            public override void Write(Utf8JsonWriter w, Rectangle v, JsonSerializerOptions o)
            {
                w.WriteStartObject();
                w.WriteNumber("X", v.X); w.WriteNumber("Y", v.Y); w.WriteNumber("Width", v.Width); w.WriteNumber("Height", v.Height);
                w.WriteEndObject();
            }
        }

        /// <summary>Cor como "#RRGGBBAA".</summary>
        private sealed class ColorConverter : JsonConverter<Color>
        {
            public override Color Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
                => ParseColor(r.GetString()) ?? Color.White;
            public override void Write(Utf8JsonWriter w, Color c, JsonSerializerOptions o)
                => w.WriteStringValue(FormatColor(c));
        }

        public static string FormatColor(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}";

        public static Color? ParseColor(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim().TrimStart('#');
            if (s.Length != 6 && s.Length != 8) return null;
            if (!uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v)) return null;
            return s.Length == 6
                ? new Color((byte)(v >> 16), (byte)(v >> 8), (byte)v, (byte)255)
                : new Color((byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v);
        }

        private static float Num(JsonElement e, string name)
            => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.TryGetSingle(out var f) ? f : 0f;
    }

    /// <summary>
    /// Componente cujo tipo não está registrado (ex.: cena aberta no editor sem a DLL do jogo).
    /// Não faz nada em runtime, mas preserva os dados para não perdê-los ao salvar.
    /// </summary>
    public sealed class MissingComponent : SceneComponent
    {
        private readonly CustomComponentData _data;

        public MissingComponent(CustomComponentData data)
        {
            _data = data;
            Enabled = data.Enabled;
        }

        /// <summary>Identificador do tipo ausente.</summary>
        public string TypeId => _data.Type;

        public override string DisplayName => "? " + _data.Type;

        public CustomComponentData ToData() => new()
        {
            Type = _data.Type,
            Enabled = Enabled,
            Properties = new Dictionary<string, JsonElement>(_data.Properties)
        };
    }
}
