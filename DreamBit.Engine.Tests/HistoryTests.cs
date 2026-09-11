using DreamBit.Engine.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DreamBit.Engine.Tests
{
    [TestClass]
    public class HistoryTests
    {
        [TestMethod]
        public void Do_ExecutaEHabilitaUndo()
        {
            var history = new History();
            int value = 0;

            history.Do(new EditorAction("+1", () => value = 1, () => value = 0));

            Assert.AreEqual(1, value);
            Assert.IsTrue(history.CanUndo);
            Assert.IsFalse(history.CanRedo);
        }

        [TestMethod]
        public void Undo_Redo_RestauraEstado()
        {
            var history = new History();
            int value = 0;

            history.Do(new EditorAction("set 5", () => value = 5, () => value = 0));
            history.Undo();
            Assert.AreEqual(0, value);
            Assert.IsTrue(history.CanRedo);

            history.Redo();
            Assert.AreEqual(5, value);
        }

        [TestMethod]
        public void NovaAcao_LimpaORedo()
        {
            var history = new History();
            int value = 0;

            history.Do(new EditorAction("a", () => value = 1, () => value = 0));
            history.Undo();
            history.Do(new EditorAction("b", () => value = 2, () => value = 0));

            Assert.IsFalse(history.CanRedo);
            Assert.AreEqual(2, value);
        }

        [TestMethod]
        public void Push_RegistraSemReexecutar()
        {
            var history = new History();
            int applied = 0;

            // simula um arraste já aplicado: Push não deve chamar Do
            history.Push(new EditorAction("mover", () => applied++, () => applied--));

            Assert.AreEqual(0, applied);
            Assert.IsTrue(history.CanUndo);

            history.Undo();
            Assert.AreEqual(-1, applied);
        }
    }
}
