using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Scrawlbit.Collections
{
    /// <summary>
    /// ObservableCollection com operações em lote (AddRange/RemoveRange/Reset) que
    /// disparam uma única notificação de Reset, evitando N eventos ao mexer em muitos itens.
    /// </summary>
    public class ExtendedObservableCollection<T> : ObservableCollection<T>
    {
        public ExtendedObservableCollection() { }
        public ExtendedObservableCollection(IEnumerable<T> items) : base(items) { }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) return;
            bool changed = false;
            foreach (var item in items)
            {
                Items.Add(item);
                changed = true;
            }
            if (changed)
                RaiseReset();
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            if (items == null) return;
            bool changed = false;
            foreach (var item in items.ToList())
                changed |= Items.Remove(item);
            if (changed)
                RaiseReset();
        }

        /// <summary>Substitui todo o conteúdo por <paramref name="items"/> com uma única notificação.</summary>
        public void Reset(IEnumerable<T> items)
        {
            Items.Clear();
            foreach (var item in items)
                Items.Add(item);
            RaiseReset();
        }

        private void RaiseReset()
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
