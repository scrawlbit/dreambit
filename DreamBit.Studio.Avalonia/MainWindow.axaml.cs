using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DreamBit.Studio.ViewModels;
using XnaGameTime = Microsoft.Xna.Framework.GameTime;

namespace DreamBit.Studio.Avalonia
{
    public partial class MainWindow : Window
    {
        private readonly EditorViewModel _editor = new();
        private readonly DispatcherTimer _timer;
        private DateTime _lastTick = DateTime.UtcNow;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _editor;

            var sceneView = this.FindControl<SceneView>("Scene");
            if (sceneView != null)
                sceneView.Editor = _editor;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += OnTick;
            _timer.Start();

            // Rola o console para o fim quando chega log novo (após o layout medir a linha).
            _editor.Log.Entries.CollectionChanged += (_, _) =>
                Dispatcher.UIThread.Post(() =>
                {
                    var scroll = this.FindControl<ScrollViewer>("ConsoleScroll");
                    if (scroll != null)
                        scroll.Offset = scroll.Offset.WithY(scroll.Extent.Height);
                }, DispatcherPriority.Background);
        }

        private void InvalidateScene() => this.FindControl<SceneView>("Scene")?.InvalidateVisual();

        private void OnTick(object? sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            var dt = now - _lastTick;
            _lastTick = now;

            if (_editor.IsPlaying)
                _editor.Scene.Update(new XnaGameTime(TimeSpan.Zero, dt));

            InvalidateScene();
        }

        private void OnPlayToggle(object? sender, RoutedEventArgs e)
        {
            _editor.IsPlaying = !_editor.IsPlaying;
            var button = this.FindControl<Button>("PlayButton");
            if (button != null)
                button.Content = _editor.IsPlaying ? "⏸ Stop" : "▶ Play";
        }

        // ---- projeto / assets / cena ----

        private async void OnOpenProject(object? sender, RoutedEventArgs e)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Abrir pasta do projeto",
                AllowMultiple = false
            });

            var path = folders.FirstOrDefault()?.TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
                _editor.OpenProjectFolder(path);
        }

        private async void OnSaveScene(object? sender, RoutedEventArgs e)
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Salvar cena",
                DefaultExtension = "dbscene",
                SuggestedFileName = _editor.Scene.Name,
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Cena DreamBit") { Patterns = new[] { "*.dbscene" } }
                }
            });

            var path = file?.TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
                _editor.SaveTo(path);
        }

        private AtlasPicker? _atlasPicker;

        private void OnOpenAtlas(object? sender, RoutedEventArgs e)
        {
            if (_atlasPicker == null)
            {
                _atlasPicker = new AtlasPicker(_editor);
                _atlasPicker.Closed += (_, _) => _atlasPicker = null;
                _atlasPicker.Show(this);
            }
            else
            {
                _atlasPicker.Activate();
            }
        }

        private void OnExitStamp(object? sender, RoutedEventArgs e) => _editor.ClearStamp();

        private void OnAssetActivated(object? sender, RoutedEventArgs e)
        {
            if (this.FindControl<ListBox>("AssetsList")?.SelectedItem is string asset)
            {
                _editor.UseAsset(asset);
                _editor.Inspector.Refresh();
                InvalidateScene();
            }
        }

        // ---- adicionar componentes ----

        private void OnAddSprite(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddSprite);
        private void OnAddAnimator(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddAnimator);
        private void OnAddPlatformer(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddPlatformer);
        private void OnAddAudio(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddAudio);
        private void OnAddParticles(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddParticles);
        private void OnAddTrigger(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddTrigger);
        private void OnAddFollow(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddFollow);
        private void OnAddRotator(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddRotator);
        private void OnAddBone(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddBone);
        private void OnAddSkeleton(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddSkeleton);
        private void OnAddScript(object? sender, RoutedEventArgs e) => AddComponent(_editor.AddScript);

        // ---- remover componentes ----

        private void OnRemoveSprite(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveSprite);
        private void OnRemoveAnimator(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveAnimator);
        private void OnRemovePlatformer(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemovePlatformer);
        private void OnRemoveAudio(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveAudio);
        private void OnRemoveParticles(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveParticles);
        private void OnRemoveTrigger(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveTrigger);
        private void OnRemoveFollow(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveFollow);
        private void OnRemoveRotator(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveRotator);
        private void OnRemoveBone(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveBone);
        private void OnRemoveSkeleton(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveSkeleton);
        private void OnRemoveScript(object? sender, RoutedEventArgs e) => AddComponent(_editor.RemoveScript);

        private void OnCompileScript(object? sender, RoutedEventArgs e) => _editor.Inspector.CompileScript();
        private void OnCaptureBonePose(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.CaptureBonePose);
        private void OnResetBonePose(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.ResetBonePose);

        private void OnAddSkeletonKeyframe(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.AddSkeletonKeyframe);
        private void OnRemoveSkeletonKeyframe(object? sender, RoutedEventArgs e) => AddComponent(_editor.Inspector.RemoveSkeletonKeyframe);

        private void OnGoToKeyframe(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: float time })
            {
                _editor.Inspector.GoToSkeletonTime(time);
                InvalidateScene();
            }
        }

        /// <summary>Executa a operação de componente e atualiza inspetor + cena.</summary>
        private void AddComponent(Action operation)
        {
            operation();
            _editor.Inspector.Refresh();
            InvalidateScene();
        }
    }
}
