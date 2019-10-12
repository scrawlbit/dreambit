using System;

namespace DreamBit.Game.Data
{
    internal class GameScript
    {
        private Type _type;

        public Guid FileId { get; set; }
        public string TypeName { get; set; }

        public Type Type
        {
            get
            {
                if (_type == null)
                    _type = Type.GetType(TypeName);

                return _type;
            }
        }
    }
}