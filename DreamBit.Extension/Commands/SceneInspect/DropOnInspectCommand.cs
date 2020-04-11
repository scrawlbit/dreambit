using DreamBit.Extension.Helpers;
using DreamBit.Extension.Management;
using DreamBit.Game.Elements;
using DreamBit.Game.Elements.Components;
using DreamBit.General.State;
using DreamBit.Pipeline.Files;
using DreamBit.Project;
using DreamBit.Project.Helpers;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Commands;
using Scrawlbit.Presentation.DragAndDrop;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
            if (_editor.SelectedObjects.Count != 1)
                return false;

            foreach (var item in args.Data)
            {
                if (!(item is string path)) return false;
                if (!path.StartsWith(_project.Folder)) return false;
            }

            return true;
        }
        public void Execute(DropEventArgs args)
        {
            ProjectFile[] files = _project.Files.GetByPaths(args.Data.Cast<string>()).NotNulls().ToArray();
            GameObject gameObject = _editor.SelectedObject;
            var components = new List<GameObjectComponent>();

            foreach (var file in files)
            {
                if (file is PipelineImage image)
                    components.Add(new ImageRenderer { Image = image });
            }

            using (_state.Scope($"Components added to {gameObject.Name}"))
            {
                foreach (var component in components)
                {
                    string name = component.GetType().Name;
                    string description = $"{name} added to {gameObject.Name}";
                    IStateCommand command = gameObject.Components.State().Add(component, description);

                    _state.Execute(command);
                }
            }
        }
    }
}
