using System;
using System.Collections.Generic;

namespace DreamBit.Game.Elements.Components
{
    internal class EditableScriptBehavior : GameObjectComponent
    {
        internal EditableScriptBehavior(Guid fileId)
        {
            FileId = fileId;
            Properties = new Dictionary<string, object>();
        }

        public ComponentType Type => ComponentType.ScriptBehavior;
        public Guid FileId { get; }
        public Dictionary<string, object> Properties { get; }
    }
}