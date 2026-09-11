using System;
using System.Collections.Generic;

namespace DreamBit.Engine.Editing
{
    /// <summary>
    /// Pilha de undo/redo do editor. Cumpre o papel do StateManager de DreamBit.General,
    /// reescrito de forma enxuta para o editor standalone.
    /// </summary>
    public sealed class History
    {
        private readonly Stack<IEditorAction> _undo = new();
        private readonly Stack<IEditorAction> _redo = new();

        public event Action? Changed;

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        /// <summary>Executa a ação e a registra para poder ser desfeita.</summary>
        public void Do(IEditorAction action)
        {
            action.Do();
            _undo.Push(action);
            _redo.Clear();
            Changed?.Invoke();
        }

        /// <summary>Registra uma ação já aplicada (ex.: fim de um arraste), sem reexecutá-la.</summary>
        public void Push(IEditorAction action)
        {
            _undo.Push(action);
            _redo.Clear();
            Changed?.Invoke();
        }

        public void Undo()
        {
            if (!CanUndo)
                return;

            var action = _undo.Pop();
            action.Undo();
            _redo.Push(action);
            Changed?.Invoke();
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            var action = _redo.Pop();
            action.Do();
            _undo.Push(action);
            Changed?.Invoke();
        }

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
            Changed?.Invoke();
        }
    }
}
