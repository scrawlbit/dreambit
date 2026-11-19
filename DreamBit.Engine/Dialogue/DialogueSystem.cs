using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DreamBit.Engine.Components;
using DreamBit.Engine.Components.Custom;
using DreamBit.Engine.Elements;
using DreamBit.Engine.Levels;
using DreamBit.Engine.Rendering;
using DreamBit.Engine.Saving;
using Microsoft.Xna.Framework;

namespace DreamBit.Engine.Dialogue
{
    /// <summary>Uma escolha de resposta.</summary>
    public sealed class DialogueChoice
    {
        public string Text { get; set; } = "";
        /// <summary>Chave de localização (opcional; tem prioridade sobre <see cref="Text"/>).</summary>
        public string? Key { get; set; }
        /// <summary>Rótulo para onde ir (vazio = próxima fala).</summary>
        public string? Goto { get; set; }
        /// <summary>Flag gravada ao escolher.</summary>
        public string? SetFlag { get; set; }
        /// <summary>Só aparece se a flag existir.</summary>
        public string? RequireFlag { get; set; }
    }

    /// <summary>Uma fala do diálogo.</summary>
    public sealed class DialogueLine
    {
        /// <summary>Rótulo (alvo de Goto).</summary>
        public string? Label { get; set; }
        public string Speaker { get; set; } = "";
        /// <summary>Retrato (imagem) mostrado na caixa.</summary>
        public string? Portrait { get; set; }
        /// <summary>Lado do retrato: "left" ou "right".</summary>
        public string Side { get; set; } = "left";
        public string Text { get; set; } = "";
        public string? Key { get; set; }
        public List<DialogueChoice> Choices { get; set; } = new();
        /// <summary>Depois desta fala, vai para o rótulo (vazio = próxima; "end" = termina).</summary>
        public string? Goto { get; set; }
        /// <summary>Flag gravada ao mostrar a fala.</summary>
        public string? SetFlag { get; set; }
        /// <summary>A fala é pulada se a flag NÃO existir.</summary>
        public string? RequireFlag { get; set; }
        /// <summary>A fala é pulada se a flag existir.</summary>
        public string? SkipIfFlag { get; set; }
        /// <summary>Mensagem enviada à cena ao mostrar a fala (gatilho de cutscene, som, animação).</summary>
        public string? Event { get; set; }
    }

    /// <summary>Diálogo (arquivo .dialogue.json): lista de falas com rótulos, escolhas e flags.</summary>
    public sealed class DialogueAsset
    {
        public string Id { get; set; } = "";
        public List<DialogueLine> Lines { get; set; } = new();

        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static DialogueAsset Parse(string json) => JsonSerializer.Deserialize<DialogueAsset>(json, Options) ?? new DialogueAsset();

        public static DialogueAsset Load(string path) => Parse(File.ReadAllText(Assets.AssetPaths.Resolve(path)));

        public string ToJson() => JsonSerializer.Serialize(this, Options);
    }

    /// <summary>
    /// Executa um diálogo: avança falas, aplica flags/eventos, resolve escolhas e trava o
    /// controle dos personagens enquanto roda (<see cref="LockPlayers"/>). Uma
    /// <see cref="DialogueBox"/> desenha o diálogo ativo.
    /// </summary>
    public static class DialogueRunner
    {
        private static DialogueAsset? _asset;
        private static int _index = -1;
        private static Scene? _scene;
        private static readonly List<PlatformerController> _locked = new();

        public static bool IsRunning => _asset != null && _index >= 0;
        public static DialogueAsset? Current => _asset;
        public static DialogueLine? Line => IsRunning ? _asset!.Lines[_index] : null;

        /// <summary>Escolhas visíveis da fala atual (filtradas por RequireFlag).</summary>
        public static IReadOnlyList<DialogueChoice> Choices
            => Line?.Choices.Where(c => string.IsNullOrEmpty(c.RequireFlag) || WorldState.HasFlag(c.RequireFlag)).ToList()
               ?? (IReadOnlyList<DialogueChoice>)Array.Empty<DialogueChoice>();

        /// <summary>Trava os PlatformerControllers da cena durante o diálogo.</summary>
        public static bool LockPlayers { get; set; } = true;

        public static event Action<DialogueLine>? LineShown;
        public static event Action<DialogueAsset>? Started;
        public static event Action<DialogueAsset>? Finished;

        /// <summary>Texto da fala/escolha já localizado.</summary>
        public static string TextOf(DialogueLine line) => Localize(line.Key, line.Text);
        public static string TextOf(DialogueChoice choice) => Localize(choice.Key, choice.Text);

        private static string Localize(string? key, string text)
            => !string.IsNullOrEmpty(key) && Localization.Localizer.Has(key) ? Localization.Localizer.Get(key) : text;

        /// <summary>Inicia um diálogo (opcionalmente a partir de um rótulo).</summary>
        public static void Start(DialogueAsset asset, Scene? scene = null, string? label = null)
        {
            if (IsRunning) Stop();
            _asset = asset;
            _scene = scene;
            if (LockPlayers && scene != null)
                foreach (var pc in Bodies.All(scene.Objects).SelectMany(o => o.Components.OfType<PlatformerController>()))
                    if (!pc.InputLocked) { pc.InputLocked = true; _locked.Add(pc); }
            Started?.Invoke(asset);
            Show(label != null ? IndexOf(label) : 0);
        }

        public static void Start(string path, Scene? scene = null, string? label = null) => Start(DialogueAsset.Load(path), scene, label);

        /// <summary>Avança para a próxima fala (ignorado se a fala tem escolhas).</summary>
        public static void Advance()
        {
            var line = Line;
            if (line == null || Choices.Count > 0) return;
            Show(Next(line.Goto));
        }

        /// <summary>Escolhe a opção <paramref name="index"/> das escolhas visíveis.</summary>
        public static void Choose(int index)
        {
            var choices = Choices;
            if (index < 0 || index >= choices.Count) return;
            var c = choices[index];
            if (!string.IsNullOrEmpty(c.SetFlag)) WorldState.SetFlag(c.SetFlag);
            Show(Next(c.Goto));
        }

        public static void Stop()
        {
            var asset = _asset;
            _asset = null;
            _index = -1;
            foreach (var pc in _locked) pc.InputLocked = false;
            _locked.Clear();
            if (asset != null) Finished?.Invoke(asset);
        }

        private static int Next(string? go)
        {
            if (string.Equals(go, "end", StringComparison.OrdinalIgnoreCase)) return -1;
            if (!string.IsNullOrEmpty(go)) return IndexOf(go);
            return _index + 1;
        }

        private static int IndexOf(string label)
            => _asset?.Lines.FindIndex(l => string.Equals(l.Label, label, StringComparison.Ordinal)) ?? -1;

        private static void Show(int index)
        {
            // Pula falas condicionadas.
            while (_asset != null && index >= 0 && index < _asset.Lines.Count)
            {
                var l = _asset.Lines[index];
                bool skip = (!string.IsNullOrEmpty(l.RequireFlag) && !WorldState.HasFlag(l.RequireFlag)) ||
                            (!string.IsNullOrEmpty(l.SkipIfFlag) && WorldState.HasFlag(l.SkipIfFlag));
                if (!skip) break;
                index = string.IsNullOrEmpty(l.Goto) ? index + 1 : (l.Goto == "end" ? -1 : IndexOf(l.Goto));
            }

            if (_asset == null || index < 0 || index >= _asset.Lines.Count)
            {
                Stop();
                return;
            }
            _index = index;
            var line = _asset.Lines[index];
            if (!string.IsNullOrEmpty(line.SetFlag)) WorldState.SetFlag(line.SetFlag);
            if (!string.IsNullOrEmpty(line.Event)) _scene?.Send(line.Event);
            LineShown?.Invoke(line);
        }
    }

    /// <summary>
    /// Caixa de diálogo (HUD): painel, retrato, nome, texto com efeito máquina de escrever e
    /// escolhas. Confirm completa/avança; MoveUp/MoveDown navegam nas escolhas.
    /// </summary>
    [GameComponent("UI/Dialogue Box", Id = "dreambit.dialoguebox")]
    public sealed class DialogueBox : SceneComponent
    {
        private DialogueLine? _shown;
        private float _chars;
        private int _choice;

        public override string DisplayName => "Dialogue Box";

        public string Font { get; set; } = "";
        public float FontSize { get; set; } = 28f;
        public float NameSize { get; set; } = 30f;
        /// <summary>Caracteres por segundo (0 = instantâneo).</summary>
        public float CharsPerSecond { get; set; } = 45f;
        /// <summary>Altura do painel (px de tela) e margem.</summary>
        public float PanelHeight { get; set; } = 220f;
        public float Margin { get; set; } = 32f;
        public float PortraitSize { get; set; } = 180f;
        /// <summary>Espaço entre a borda do painel e o texto (horizontal / vertical). Painéis com
        /// moldura grossa ou cantos arredondados precisam de mais.</summary>
        public float TextPaddingX { get; set; } = 56f;
        public float TextPaddingY { get; set; } = 36f;
        /// <summary>Espaço entre a borda do painel e o retrato, e entre o retrato e o texto.</summary>
        public float PortraitInset { get; set; } = 30f;
        public Color PanelColor { get; set; } = new(18, 16, 24, 220);
        public Color TextColor { get; set; } = new(245, 238, 225);
        public Color NameColor { get; set; } = new(255, 196, 120);
        public Color ChoiceColor { get; set; } = new(200, 190, 175);
        public Color SelectedColor { get; set; } = new(255, 220, 140);
        /// <summary>Imagem de fundo do painel (opcional; esticada).</summary>
        [AssetPath("*.png")]
        public string PanelTexture { get; set; } = "";
        /// <summary>Jogador que avança o diálogo (-1 = global).</summary>
        public int PlayerIndex { get; set; } = -1;

        /// <summary>Caracteres visíveis da fala atual.</summary>
        public int VisibleChars => (int)_chars;

        protected internal override void Update(GameTime gameTime)
        {
            var line = DialogueRunner.Line;
            if (line == null) { _shown = null; return; }
            if (!ReferenceEquals(line, _shown))
            {
                _shown = line;
                _chars = CharsPerSecond <= 0f ? int.MaxValue : 0f;
                _choice = 0;
                return; // o Confirm que abriu/avançou não pula a fala nova
            }

            var text = DialogueRunner.TextOf(line);
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_chars < text.Length) _chars += CharsPerSecond * dt;

            var input = Input.Input.ForPlayer(PlayerIndex);
            var choices = DialogueRunner.Choices;
            if (choices.Count > 0 && _chars >= text.Length)
            {
                if (input.JustPressed("MoveDown")) _choice = (_choice + 1) % choices.Count;
                if (input.JustPressed("MoveUp")) _choice = (_choice + choices.Count - 1) % choices.Count;
            }

            if (input.JustPressed("Confirm") || input.JustPressed("Jump") || input.JustPressed("Action"))
            {
                if (_chars < text.Length) _chars = text.Length;
                else if (choices.Count > 0) DialogueRunner.Choose(_choice);
                else DialogueRunner.Advance();
            }
        }

        protected internal override void DrawScreen(ISceneDrawing drawing)
        {
            var line = DialogueRunner.Line;
            if (line == null) return;

            float w = Screen.Width, h = Screen.Height;
            var panel = new Rectangle((int)Margin, (int)(h - PanelHeight - Margin), (int)(w - Margin * 2), (int)PanelHeight);
            var tex = string.IsNullOrEmpty(PanelTexture) ? null : TextureCache.Get(drawing.SpriteBatch.GraphicsDevice, PanelTexture);
            drawing.SpriteBatch.Draw(tex ?? drawing.Pixel, panel, tex != null ? Color.White : PanelColor);

            float textLeft = panel.X + TextPaddingX, textRight = panel.Right - TextPaddingX;
            if (!string.IsNullOrEmpty(line.Portrait) && TextureCache.Get(drawing.SpriteBatch.GraphicsDevice, line.Portrait) is { } portrait)
            {
                bool right = string.Equals(line.Side, "right", StringComparison.OrdinalIgnoreCase);
                // O retrato cabe no painel com a mesma folga em cima, embaixo e na lateral.
                float fit = Math.Min(PortraitSize, panel.Height - PortraitInset * 2f);
                float scale = fit / Math.Max(portrait.Width, portrait.Height);
                var size = new Vector2(portrait.Width, portrait.Height) * scale;
                var pos = new Vector2(right ? panel.Right - size.X - PortraitInset : panel.X + PortraitInset, panel.Bottom - size.Y - PortraitInset);
                drawing.SpriteBatch.Draw(portrait, new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y), Color.White);
                if (right) textRight = pos.X - PortraitInset; else textLeft = pos.X + size.X + PortraitInset;
            }

            float y = panel.Y + TextPaddingY;
            if (!string.IsNullOrEmpty(line.Speaker))
            {
                Fonts.Draw(drawing, Font, NameSize, line.Speaker, new Vector2(textLeft, y), NameColor, Color.Black * 0.6f, 1.5f);
                y += NameSize * 1.3f;
            }

            var full = DialogueRunner.TextOf(line);
            var visible = full.Substring(0, Math.Min(full.Length, Math.Max(0, VisibleChars)));
            var wrapped = Fonts.Wrap(Font, FontSize, visible, textRight - textLeft);
            Fonts.Draw(drawing, Font, FontSize, wrapped, new Vector2(textLeft, y), TextColor);

            var choices = DialogueRunner.Choices;
            if (choices.Count > 0 && VisibleChars >= full.Length)
            {
                float cy = y + Fonts.Measure(Font, FontSize, wrapped).Y + FontSize * 0.6f;
                for (int i = 0; i < choices.Count; i++)
                {
                    var color = i == _choice ? SelectedColor : ChoiceColor;
                    Fonts.Draw(drawing, Font, FontSize, (i == _choice ? "> " : "  ") + DialogueRunner.TextOf(choices[i]), new Vector2(textLeft + 12, cy), color);
                    cy += FontSize * 1.25f;
                }
            }
        }
    }

    /// <summary>
    /// Personagem/objeto com quem se conversa: com um jogador perto e a ação apertada (ou ao
    /// entrar na área, se <see cref="AutoStart"/>), inicia o diálogo.
    /// </summary>
    [GameComponent("Mundo/Talk Trigger", Id = "dreambit.talk")]
    public sealed class TalkTrigger : SceneComponent
    {
        private bool _wasInside;

        public override string DisplayName => "Talk Trigger";

        [AssetPath("*.json")]
        public string DialoguePath { get; set; } = "";
        /// <summary>Rótulo inicial (vazio = começo).</summary>
        public string StartLabel { get; set; } = "";
        public Vector2 Size { get; set; } = new(160, 160);
        public string Tag { get; set; } = "Player";
        public string Action { get; set; } = "Action";
        /// <summary>Começa sozinho ao entrar na área (cutscene).</summary>
        public bool AutoStart { get; set; }
        /// <summary>Só uma vez (grava a flag "talk.&lt;nome do objeto&gt;").</summary>
        public bool Once { get; set; }
        /// <summary>Diálogo inline (usado se DialoguePath estiver vazio).</summary>
        [HideInInspector]
        public DialogueAsset? Inline { get; set; }

        private string OnceFlag => "talk." + Owner.Name;

        public void Start()
        {
            if (DialogueRunner.IsRunning) return;
            var asset = Inline ?? (string.IsNullOrEmpty(DialoguePath) ? null : DialogueAsset.Load(DialoguePath));
            if (asset == null) return;
            if (Once) WorldState.SetFlag(OnceFlag);
            DialogueRunner.Start(asset, Owner.Scene, string.IsNullOrEmpty(StartLabel) ? null : StartLabel);
        }

        protected internal override void Update(GameTime gameTime)
        {
            if (Owner.Scene == null || DialogueRunner.IsRunning || (Once && WorldState.HasFlag(OnceFlag)))
                return;
            var who = Bodies.InArea(this, Size, Tag).FirstOrDefault();
            bool inside = who != null;
            bool entered = inside && !_wasInside;
            _wasInside = inside;
            if (who == null) return;

            if (AutoStart ? entered : (who.Components.OfType<Input.IInputControlled>().FirstOrDefault()?.InputSource ?? Input.Input.Global).JustPressed(Action))
                Start();
        }

        protected internal override void Draw(ISceneDrawing drawing)
            => Bodies.DrawArea(drawing, Owner, Size, new Color(255, 230, 120) * 0.15f);
    }

    /// <summary>Texto com fonte TrueType (HUD ou rótulo no mundo).</summary>
    [GameComponent("UI/Font Text", Id = "dreambit.fonttext")]
    public sealed class FontText : SceneComponent
    {
        public override string DisplayName => "Font Text";

        public string Text { get; set; } = "Texto";
        /// <summary>Chave de localização (opcional; tem prioridade sobre Text).</summary>
        public string LocKey { get; set; } = "";
        public string Font { get; set; } = "";
        public float Size { get; set; } = 32f;
        public Color Color { get; set; } = Color.White;
        public bool Outline { get; set; } = true;
        public Color OutlineColor { get; set; } = new(0, 0, 0, 180);
        /// <summary>Quebra de linha (px); 0 = sem quebra.</summary>
        public float MaxWidth { get; set; }
        /// <summary>Centralizado no objeto (no mundo) ou a partir do canto (HUD).</summary>
        public bool Centered { get; set; } = true;
        /// <summary>Desenha em espaço de tela (HUD).</summary>
        public bool ScreenSpace { get; set; }

        public string DisplayText => !string.IsNullOrEmpty(LocKey) && Localization.Localizer.Has(LocKey) ? Localization.Localizer.Get(LocKey) : Text;

        protected internal override void Draw(ISceneDrawing drawing)
        {
            if (!ScreenSpace) DrawAt(drawing, Owner.Transform.WorldPosition);
        }

        protected internal override void DrawScreen(ISceneDrawing drawing)
        {
            if (ScreenSpace) DrawAt(drawing, Owner.Transform.Position);
        }

        private void DrawAt(ISceneDrawing drawing, Vector2 at)
        {
            var text = Fonts.Wrap(Font, Size, DisplayText, MaxWidth);
            if (Centered) at -= Fonts.Measure(Font, Size, text) / 2f;
            Fonts.Draw(drawing, Font, Size, text, at, Color, Outline ? OutlineColor : null);
        }
    }
}
