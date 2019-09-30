using System.Text;
using System.Text.RegularExpressions;
using DreamBit.Modularization.Management;
using DreamBit.Project;
using Scrawlbit.Helpers;

namespace DreamBit.Pipeline.Files
{
    public interface IPipelineFont : IProjectFile
    {
        FontFamily Family { get; }
        int Size { get; }
        int Spacing { get; }
        FontStyle Style { get; }

        void Load();
        void Save();
    }

    public sealed class PipelineFont : ProjectFile, IPipelineFont
    {
        internal const string FontType = "Font";
        internal const string FontExtension = "spritefont";

        private readonly IFileManager _fileManager;
        private bool _loaded;
        private FontFamily _family;
        private int _size;
        private int _spacing;
        private FontStyle _style;

        public PipelineFont(IFileManager fileManager)
        {
            _fileManager = fileManager;
            _family = FontFamily.SegoeUI;
            _size = 12;
            _spacing = 0;
            _style = FontStyle.Regular;
        }

        public override string Extension => FontExtension;
        public override string Type => FontType;
        public FontFamily Family
        {
            get
            {
                LoadIfNotLoaded();
                return _family;
            }
            set
            {
                LoadIfNotLoaded();
                _family = value;
            }
        }
        public int Size
        {
            get
            {
                LoadIfNotLoaded();
                return _size;
            }
            set
            {
                LoadIfNotLoaded();
                _size = value;
            }
        }
        public int Spacing
        {
            get
            {
                LoadIfNotLoaded();
                return _spacing;
            }
            set
            {
                LoadIfNotLoaded();
                _spacing = value;
            }
        }
        public FontStyle Style
        {
            get
            {
                LoadIfNotLoaded();
                return _style;
            }
            set
            {
                LoadIfNotLoaded();
                _style = value;
            }
        }

        public void Load()
        {
            var text = _fileManager.ReadAllText(Path);

            string ReadNode(string node)
            {
                var match = Regex.Match(text, $"<{node}>(.+)</{node}>", RegexOptions.IgnoreCase);
                var value = match.Groups[1].Value;

                return value;
            }

            _family = EnumHelper.Parse<FontFamily>(ReadNode("FontName"));
            _size = ReadNode("Size").ToInt();
            _spacing = ReadNode("Spacing").ToInt();
            _style = EnumHelper.Parse<FontStyle>(ReadNode("Style"));

            _loaded = true;
        }
        public void Save()
        {
            var xml = new StringBuilder();

            xml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xml.AppendLine("<XnaContent xmlns:Graphics=\"Microsoft.Xna.Framework.Content.Pipeline.Graphics\">");
            xml.AppendLine("  <Asset Type=\"Graphics:FontDescription\">");
            xml.AppendLine($"    <FontName>{Family}</FontName>");
            xml.AppendLine($"    <Size>{Size}</Size>");
            xml.AppendLine($"    <Spacing>{Spacing}</Spacing>");
            xml.AppendLine("    <UseKerning>true</UseKerning>");
            xml.AppendLine($"    <Style>{Style}</Style>");
            xml.AppendLine("    <CharacterRegions>");
            xml.AppendLine("      <CharacterRegion>");
            xml.AppendLine("        <Start>&#32;</Start>");
            xml.AppendLine("        <End>&#126;</End>");
            xml.AppendLine("      </CharacterRegion>");
            xml.AppendLine("    </CharacterRegions>");
            xml.AppendLine("  </Asset>");
            xml.Append("</XnaContent>");

            _fileManager.WriteAllText(Path, xml.ToString(), new UTF8Encoding(true));
        }

        private void LoadIfNotLoaded()
        {
            if (!_loaded)
                Load();
        }
    }
}