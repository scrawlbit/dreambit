using DreamBit.Engine.Editing;
using Xunit;

namespace DreamBit.Engine.Tests
{
    public class HistoryTests
    {
        [Fact]
        public void Do_ExecutaEHabilitaUndo()
        {
            var history = new History();
            int value = 0;

            history.Do(new EditorAction("+1", () => value = 1, () => value = 0));

            Assert.Equal(1, value);
            Assert.True(history.CanUndo);
            Assert.False(history.CanRedo);
        }

        [Fact]
        public void Undo_Redo_RestauraEstado()
        {
            var history = new History();
            int value = 0;

            history.Do(new EditorAction("set 5", () => value = 5, () => value = 0));
            history.Undo();
            Assert.Equal(0, value);
            Assert.True(history.CanRedo);

            history.Redo();
            Assert.Equal(5, value);
        }

        [Fact]
        public void NovaAcao_LimpaORedo()
        {
            var history = new History();
            int value = 0;

            history.Do(new EditorAction("a", () => value = 1, () => value = 0));
            history.Undo();
            history.Do(new EditorAction("b", () => value = 2, () => value = 0));

            Assert.False(history.CanRedo);
            Assert.Equal(2, value);
        }

        [Fact]
        public void Push_RegistraSemReexecutar()
        {
            var history = new History();
            int applied = 0;

            // simula um arraste já aplicado: Push não deve chamar Do
            history.Push(new EditorAction("mover", () => applied++, () => applied--));

            Assert.Equal(0, applied);
            Assert.True(history.CanUndo);

            history.Undo();
            Assert.Equal(-1, applied);
        }
    }
}
