using DreamBit.Extension.Commands;
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
using System.Runtime.InteropServices;
using System.Threading;
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
            await RegisterCommandAsync<BuildContentCommand>();
            await RegisterCommandAsync<SceneEditorWindowCommand>();
            await RegisterCommandAsync<SceneHierarchyWindowCommand>();
            await RegisterCommandAsync<SceneInspectWindowCommand>();
            await RegisterCommandAsync<AddFontCommand>();
            await RegisterCommandAsync<AddGameObjectCommand>();
            await RegisterCommandAsync<AddCameraObjectCommand>();
            await RegisterCommandAsync<AddSceneCommand>();
        }

        private IEnumerable<IInjectionModule> GetModules()
        {
            yield return new ExtensionInjectionModule(this, Mapper);
            yield return new ProjectInjectionModule();
            yield return new PipelineInjectionModule();
            yield return new GeneralInjectionModule();
            yield return new GameEditionInjectionModule();
        }
        private Task RegisterCommandAsync<T>() where T : class, IToolCommand
        {
            return Container.Resolve<T>().RegisterAsync(_bridge);
        }
    }
}
