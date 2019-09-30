using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Scrawlbit.Presentation.DragAndDrop
{
    public class TreeViewDropEventArgs : DropEventArgs
    {
        public TreeViewDropEventArgs()
        {
            Sources = new Dictionary<object, SourceData>();
        }

        public object OriginalTarget { get; set; }
        public IDictionary<object, SourceData> Sources { get; }
        public IEnumerable To { get; set; }
        public int ToIndex { get; set; }

        public SourceData SingleSource => Sources.Single().Value;
        
        public class SourceData
        {
            public object Data { get; set; }
            public IEnumerable From { get; set; }
            public int FromIndex { get; set; }
        }
    }
}