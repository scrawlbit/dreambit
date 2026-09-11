using System;

namespace DreamBit.Engine.Editing
{
    /// <summary>Uma ação reversível do editor (para undo/redo).</summary>
    public interface IEditorAction
    {
        string Name { get; }
        void Do();
        void Undo();
    }

    /// <summary>Ação reversível baseada em delegates.</summary>
    public sealed class EditorAction : IEditorAction
    {
        private readonly Action _do;
        private readonly Action _undo;

        public EditorAction(string name, Action doAction, Action undoAction)
        {
            Name = name;
            _do = doAction;
            _undo = undoAction;
        }

        public string Name { get; }
        public void Do() => _do();
        public void Undo() => _undo();
    }
}
