using DreamBit.Extension.Commands;
using DreamBit.Extension.Commands.Project;
using DreamBit.Extension.Commands.SceneHierarchy;
using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Extension.ViewModels;
using DreamBit.Extension.ViewModels.Dialogs;
using DreamBit.Modularization.Management;
using Microsoft.VisualStudio.Shell;
using Scrawlbit.Injection.Configuration;
using Scrawlbit.Mapping;

namespace DreamBit.Extension.Properties
{
    internal class ExtensionInjectionModule : IInjectionModule
    {
        private readonly AsyncPackage _package;
        private readonly IMappingService _mapper;
        private IRegistrationBuilder _builder;

        public ExtensionInjectionModule(AsyncPackage package, IMappingService mapper)
        {
            _package = package;
            _mapper = mapper;
        }

        public void Register(IRegistrationBuilder builder)
        {
            _builder = builder;

            _builder.Register<AsyncPackage>().Singleton(_package);
            _builder.Register<IMappingService>().Singleton(_mapper);

            Commands();
            Components();
            Management();
            ViewModels();
        }

        private void Commands()
        {
            _builder.Register<IBuildContentCommand>().Singleton<BuildContentCommand>();
            _builder.Register<ISceneEditorWindowCommand>().Singleton<SceneEditorWindowCommand>();
            _builder.Register<ISceneHierarchyWindowCommand>().Singleton<SceneHierarchyWindowCommand>();
            _builder.Register<ISceneInspectWindowCommand>().Singleton<SceneInspectWindowCommand>();

            // Project
            _builder.Register<IAddFontCommand>().Singleton<AddFontCommand>();
            _builder.Register<IAddSceneCommand>().Singleton<AddSceneCommand>();
            _builder.Register<IEditFontCommand>().Singleton<EditFontCommand>();
            _builder.Register<IEditSceneCommand>().Singleton<EditSceneCommand>();
            _builder.Register<IAddScriptCommand>().Singleton<AddScriptCommand>();

            // SceneHierarchy
            _builder.Register<IAddCameraObjectCommand>().Singleton<AddCameraObjectCommand>();
            _builder.Register<IAddGameObjectCommand>().Singleton<AddGameObjectCommand>();
            _builder.Register<ICopyGameObjectCommand>().Singleton<CopyGameObjectCommand>();
            _builder.Register<IPasteGameObjectCommand>().Singleton<PasteGameObjectCommand>();
            _builder.Register<IRemoveGameObjectCommand>().Singleton<RemoveGameObjectCommand>();
            _builder.Register<IMoveGameObjectCommand>().Singleton<MoveGameObjectCommand>();
        }
        private void Components()
        {
            _builder.Register<IPackageBridge>().Singleton(c =>
            {
                IFileManager fileManager = c.Resolve<IFileManager>();

                return new PackageBridge(_package, fileManager);
            });
        }
        private void Management()
        {
            _builder.Register<IProjectManager>().Singleton<ProjectManager>();
            _builder.Register<IEditor>().Singleton<Editor>();
        }
        private void ViewModels()
        {
            _builder.Register<SceneHierarchyViewModel>();

            // dialogs
            _builder.Register<EditFontDialogViewModel>();
        }
    }
}