using Scrawlbit.Helpers;
using System;
using System.Reflection;

namespace Scrawlbit.Presentation.Commands
{
    internal class CommandParameter
    {
        private readonly ParameterInfo _info;
        private bool? _isNullable;

        public CommandParameter(ParameterInfo info)
        {
            _info = info;
        }

        private Type Type => _info.ParameterType;
        private bool IsNullable
        {
            get
            {
                if (_isNullable == null)
                    _isNullable = Type.IsClass || Type.IsInterface || Type.IsNullable();

                return _isNullable.Value;
            }
        }

        public bool IsAssignable(object value)
        {
            if (value == null && IsNullable)
                return true;

            return value?.GetType().IsAssignableTo(Type) == true;
        }
    }
}