using DreamBit.Extension.Commands;
using DreamBit.Extension.Commands.Editor;
using DreamBit.Extension.Commands.Project;
using DreamBit.Extension.Commands.SceneHierarchy;
using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Extension.Properties;
using DreamBit.Game.Properties;
using DreamBit.Modularization.Properties;
using DreamBit.Pipeline.Properties;
using DreamBit.Project.Properties;
using DreamBit.Resources;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Scrawlbit.Injection;
using Scrawlbit.Injection.Configuration;
using Scrawlbit.Mapping;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace DreamBit.Extension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(Guids.Package)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(Windows.SceneEditorWindow))]
    [ProvideToolWindow(typeof(Windows.SceneHierarchyWindow))]
    [ProvideToolWindow(typeof(Windows.SceneInspectWindow))]
    public sealed partial class DreamBitPackage : AsyncPackage
    {
        private IPackageBridge _bridge;

        internal static IContainer Container { get; private set; }
        internal static IMappingService Mapper { get; private set; }

        private static void BuildMapper()
        {
            var builder = new MappingServiceBuilder();

            builder.MappingBuilder.RegisterProfile<PipelineMappingProfile>();

            Mapper = builder.Build();
        }
        private void BuildContainer()
        {
            var builder = new ContainerBuilder { AllowOverrides = true };

            foreach (var module in GetModules())
                builder.RegistrationBuilder.RegisterModule(module);

            Container = builder.Build();
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            ResourcesLoader.EnsureLoad();

            BuildMapper();
            BuildContainer();

            Container.Inject(out _bridge);
            Container.Resolve<IProjectManager>().Initialize();

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await _bridge.InitializeAsync();
            await RegisterCommandAsync<IBuildContentCommand>();
            await RegisterCommandAsync<ISceneEditorWindowCommand>();
            await RegisterCommandAsync<ISceneHierarchyWindowCommand>();
            await RegisterCommandAsync<ISceneInspectWindowCommand>();
            await RegisterCommandAsync<IAddFontCommand>();
            await RegisterCommandAsync<IAddGameObjectCommand>();
            await RegisterCommandAsync<IAddCameraObjectCommand>();
            await RegisterCommandAsync<IAddSceneCommand>();
            await RegisterCommandAsync<IEditFontCommand>();
            await RegisterCommandAsync<IEditSceneCommand>();
            await RegisterCommandAsync<IAddScriptCommand>();
            await RegisterCommandAsync<IUndoCommand>();
            await RegisterCommandAsync<IRedoCommand>();
            await RegisterCommandAsync<ISaveSceneCommand>();
            await RegisterCommandAsync<ICloseSceneCommand>();

            ApplyTheme();

            Assembly.Load("Microsoft.Xaml.Behaviors");
        }

        private static void ApplyTheme()
        {
            var path = new Uri("pack://application:,,,/DreamBit.Extension;component/Resources/Styles/Theme.xaml");
            var resource = new ResourceDictionary { Source = path };

            Application.Current.Resources.MergedDictionaries.Add(resource);
        }

        private IEnumerable<IInjectionModule> GetModules()
        {
            yield return new ExtensionInjectionModule(this, Mapper);
            yield return new ProjectInjectionModule();
            yield return new PipelineInjectionModule();
            yield return new GeneralInjectionModule();
            yield return new GameInjectionModule();
        }
        private Task RegisterCommandAsync<T>() where T : class, IToolCommand
        {
            return Container.Resolve<T>().RegisterAsync(_bridge);
        }
    }
}
