using DreamBit.Extension.Helpers;
using DreamBit.Game.Elements.Components;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

#pragma warning disable VSTHRD100 // Avoid async void methods
namespace DreamBit.Extension.Windows.SceneInspect
{
    public partial class ScriptBehaviorInspect
    {
        private readonly Regex _regex;
        private ScriptBehavior _script;
        private FileSystemWatcher _watcher;

        public ScriptBehaviorInspect()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            _regex = new Regex("public (\\w+ )?(?<Type>[\\w?]+) (?<PropertyName>\\w+) ?{ ?get; ?set; ?}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DataContextProperty)
            {
                _script = DataContext as ScriptBehavior;

                if (_script != null)
                    TrackChanges();
                else
                    DisposeTrack();
            }
        }

        private void TrackChanges()
        {
            _watcher = new FileSystemWatcher();

            _watcher.Path = _script.File.Folder;
            _watcher.Filter = _script.File.FileName;
            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.EnableRaisingEvents = true;
            _watcher.Changed += OnFileChanged;
            _watcher.Renamed += OnFileRenamed;

            ReadProperties();
        }
        private void DisposeTrack()
        {
            _watcher?.Dispose();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            ReadProperties();
        }
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (e.FullPath == _script.File.Path)
                ReadProperties();
        }

        private async void ReadProperties()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            using (var stream = new FileStream(_script.File.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                string text = await reader.ReadToEndAsync();
                MatchCollection matches = _regex.Matches(text);
                var properties = new List<(string Name, string Type)>();

                foreach (Match match in matches)
                {
                    string propertyType = match.Groups["Type"].Value;
                    string propertyName = match.Groups["PropertyName"].Value;

                    properties.Add((propertyName, propertyType));
                }

                _script.MergeProperties(properties.ToArray());
            }
        }
    }
}
