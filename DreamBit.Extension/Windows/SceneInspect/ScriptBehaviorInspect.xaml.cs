using DreamBit.Extension.Controls.Input;
using DreamBit.Extension.Helpers;
using DreamBit.Game.Elements.Components;
using DreamBit.General.State;
using Microsoft.VisualStudio.Shell;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CheckBox = DreamBit.Extension.Controls.Input.CheckBox;
using TextBox = DreamBit.Extension.Controls.Input.TextBox;

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

        private void OnIntChanged(IntBox sender, ValueChangedEventArgs<int> e) => CreateState(sender, e.OldValue, e.NewValue);
        private void OnBoolChanged(CheckBox sender, ValueChangedEventArgs<bool?> e) => CreateState(sender, e.OldValue, e.NewValue);
        private void OnStringChanged(TextBox sender, ValueChangedEventArgs<string> e) => CreateState(sender, e.OldValue, e.NewValue);
        private void OnFloatChanged(FloatBox sender, ValueChangedEventArgs<float> e) => CreateState(sender, e.OldValue, e.NewValue);
        private void OnVector2Changed(FloatBox sender, ValueChangedEventArgs<float> e)
        {
            ScriptProperty property = (ScriptProperty)sender.DataContext;
            Vector2 value = (Vector2)property.Value;
            Vector2 oldValue = value;
            Vector2 newValue = value;

            if (sender.Uid == "X")
            {
                oldValue.X = e.OldValue;
                newValue.X = e.NewValue;
            }
            else
            {
                oldValue.Y = e.OldValue;
                newValue.Y = e.NewValue;
            }

            CreateState(property, oldValue, newValue);
        }
        private void OnGameObjectChanged(GameObjectSelector sender, ValueChangedEventArgs<Guid> e) => CreateState(sender, e.OldValue, e.NewValue);

        private void CreateState(Control control, object oldValue, object newValue)
        {
            CreateState((ScriptProperty)control.DataContext, oldValue, newValue);
        }
        private void CreateState(ScriptProperty property, object oldValue, object newValue)
        {
            ScriptBehavior script = _script;
            string name = property.Name;
            ScriptPropertyType type = property.Type;

            ViewModel.State.Add(new StateCommand
            {
                Description = $"{script.GameObject.Name}'s {script.Name} - {name} changed",
                Do = () => script.SetValue(name, type, newValue),
                Undo = () => script.SetValue(name, type, oldValue)
            });
        }
    }
}
