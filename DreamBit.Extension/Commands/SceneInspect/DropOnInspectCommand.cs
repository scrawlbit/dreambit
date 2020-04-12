using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using DreamBit.General.State;
using DreamBit.Pipeline.Files;
using DreamBit.Project;
using DreamBit.Project.Helpers;
using Scrawlbit.Presentation.Commands;
using Scrawlbit.Presentation.DragAndDrop;
using System;
using System.Windows.Input;

namespace DreamBit.Extension.Commands.SceneInspect
{
    public interface IDropOnInspectCommand : ICommand
    {
        bool CanExecute(DropEventArgs args);
        void Execute(DropEventArgs args);
    }

    internal class DropOnInspectCommand : BaseCommand, IDropOnInspectCommand
    {
        private readonly IEditor _editor;
        private readonly IProject _project;
        private readonly IStateManager _state;

        public DropOnInspectCommand(IEditor editor, IProject project, IStateManager state)
        {
            _editor = editor;
            _project = project;
            _state = state;
        }

        public bool CanExecute(DropEventArgs args)
        {
            if (_editor.SelectedObjects.Count != 1 && args.Data.Length == 1)
                return false;

            string path = args.Data[0] as string;

            if (path?.StartsWith(_project.Folder) != true)
                return false;

            GameObject gameObject = _editor.SelectedObject;
            ProjectFile file = _project.Files.GetByPath(path);

            return CanAddComponent(gameObject, file);
        }
        public void Execute(DropEventArgs args)
        {
            string path = (string)args.Data[0];
            ProjectFile file = _project.Files.GetByPath(path);
            GameObject gameObject = _editor.SelectedObject;
            GameComponent component = CreateComponent(file);

            string name = component.GetType().Name;
            string description = $"{name} added to {gameObject.Name}";
            IStateCommand command = gameObject.Components.State().Add(component, description);

            _state.Execute(command);
        }

        private static bool CanAddComponent(GameObject gameObject, ProjectFile file)
        {
            switch (file)
            {
                case PipelineImage _: return !gameObject.Components.Contains<ImageRenderer>();
            }

            return false;
        }
        private static GameComponent CreateComponent(ProjectFile file)
        {
            switch (file)
            {
                case PipelineImage image: return new ImageRenderer { Image = image };

                default:
                    throw new ArgumentOutOfRangeException(nameof(file));
            }
        }
    }
}
