﻿using DreamBit.Extension.Commands.Editor;
using DreamBit.Extension.Module;

namespace DreamBit.Extension.ViewModels
{
    internal class SceneEditorViewModel : BaseViewModel
    {
        public SceneEditorViewModel(
            SceneEditorGame gameModule,
            ISaveSceneCommand saveSceneCommand,
            IUndoCommand undoCommand,
            IRedoCommand redoCommand,
            ICloseSceneCommand closeSceneCommand)
        {
            GameModule = gameModule;
            SaveSceneCommand = saveSceneCommand;
            UndoCommand = undoCommand;
            RedoCommand = redoCommand;
            CloseSceneCommand = closeSceneCommand;
        }

        public SceneEditorGame GameModule { get; }
        public ISaveSceneCommand SaveSceneCommand { get; }
        public IUndoCommand UndoCommand { get; }
        public IRedoCommand RedoCommand { get; }
        public ICloseSceneCommand CloseSceneCommand { get; }
    }
}