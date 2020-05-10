using DreamBit.Extension.Commands.Editor;
using DreamBit.Extension.Module;

namespace DreamBit.Extension.ViewModels
{
    internal class SceneEditorViewModel : BaseViewModel
    {
        public SceneEditorViewModel(
            EditorGameModule gameModule,
            ISaveSceneCommand saveSceneCommand,
            IUndoCommand undoCommand,
            IRedoCommand redoCommand,
            ICloseSceneCommand closeSceneCommand,
            IZoomOutCommand zoomOutCommand,
            IZoomInCommand zoomInCommand,
            IZoomToOriginalSizeCommand zoomToOriginalSizeCommand,
            IZoomToFitScreenCommand zoomToFitScreenCommand)
        {
            GameModule = gameModule;
            SaveSceneCommand = saveSceneCommand;
            UndoCommand = undoCommand;
            RedoCommand = redoCommand;
            CloseSceneCommand = closeSceneCommand;
            ZoomOutCommand = zoomOutCommand;
            ZoomInCommand = zoomInCommand;
            ZoomToOriginalSizeCommand = zoomToOriginalSizeCommand;
            ZoomToFitScreenCommand = zoomToFitScreenCommand;
        }

        public EditorGameModule GameModule { get; }
        public ISaveSceneCommand SaveSceneCommand { get; }
        public IUndoCommand UndoCommand { get; }
        public IRedoCommand RedoCommand { get; }
        public ICloseSceneCommand CloseSceneCommand { get; }
        public IZoomOutCommand ZoomOutCommand { get; }
        public IZoomInCommand ZoomInCommand { get; }
        public IZoomToOriginalSizeCommand ZoomToOriginalSizeCommand { get; }
        public IZoomToFitScreenCommand ZoomToFitScreenCommand { get; }
    }
}
