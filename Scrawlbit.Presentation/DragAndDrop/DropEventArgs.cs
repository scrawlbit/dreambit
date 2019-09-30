﻿using System.Collections.Generic;
using System.Linq;

namespace ScrawlBit.Presentation.DragAndDrop
{
    public class DropEventArgs
    {
        public object[] Data { get; set; }
        public object Target { get; set; }
        public DropType DropType { get; set; }
        public bool IsFiles { get; set; }

        public bool HasMultipleData => Data?.Length > 1;
        public object SingleData => Data?.Single();
        public IEnumerable<string> Files => Data.Cast<string>();
    }
}