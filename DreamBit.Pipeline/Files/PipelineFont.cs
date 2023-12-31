﻿using DreamBit.Modularization.Management;
using DreamBit.Project;
using Scrawlbit.Helpers;
using System.Text;
using System.Text.RegularExpressions;

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
        private readonly IPipeline _pipeline;
        private readonly IFileManager _fileManager;
        private bool _loaded;
        private FontFamily _family;
        private int _size;
        private int _spacing;
        private FontStyle _style;

        internal PipelineFont(IPipeline pipeline, IFileManager fileManager)
        {
            _pipeline = pipeline;
            _fileManager = fileManager;
            _family = FontFamily.SegoeUI;
            _size = 12;
            _spacing = 0;
            _style = FontStyle.Regular;
        }

        public FontFamily Family
        {
            get
            {
                EnsureLoaded();
                return _family;
            }
            set
            {
                EnsureLoaded();
                _family = value;
            }
        }
        public int Size
        {
            get
            {
                EnsureLoaded();
                return _size;
            }
            set
            {
                EnsureLoaded();
                _size = value;
            }
        }
        public int Spacing
        {
            get
            {
                EnsureLoaded();
                return _spacing;
            }
            set
            {
                EnsureLoaded();
                _spacing = value;
            }
        }
        public FontStyle Style
        {
            get
            {
                EnsureLoaded();
                return _style;
            }
            set
            {
                EnsureLoaded();
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
            xml.AppendLine($"    <FontName>{_family}</FontName>");
            xml.AppendLine($"    <Size>{_size}</Size>");
            xml.AppendLine($"    <Spacing>{_spacing}</Spacing>");
            xml.AppendLine("    <UseKerning>true</UseKerning>");
            xml.AppendLine($"    <Style>{_style}</Style>");
            xml.AppendLine("    <CharacterRegions>");
            xml.AppendLine("      <CharacterRegion>");
            xml.AppendLine("        <Start>&#32;</Start>");
            xml.AppendLine("        <End>&#126;</End>");
            xml.AppendLine("      </CharacterRegion>");
            xml.AppendLine("    </CharacterRegions>");
            xml.AppendLine("  </Asset>");
            xml.Append("</XnaContent>");

            _fileManager.WriteAllText(Path, xml.ToString(), new UTF8Encoding(true));
            _loaded = true;
        }

        protected override void OnAdded()
        {
            _pipeline.Contents.AddImport(this);

            if (!_fileManager.FileExists(Path))
                Save();
        }
        protected override void OnReplaced()
        {
            _loaded = false;
        }
        protected override void OnMoved(MovedEventArgs e)
        {
            _pipeline.Contents.Move(this, e.OldLocation);
        }
        protected override void OnRemoved()
        {
            _pipeline.Contents.Remove(this);
        }

        private void EnsureLoaded()
        {
            if (!_loaded)
                Load();
        }
    }
}