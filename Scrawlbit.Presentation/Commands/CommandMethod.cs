using Scrawlbit.Helpers;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Scrawlbit.Presentation.Commands
{
    internal class CommandMethod
    {
        private readonly MethodInfo _info;
        private readonly CommandParameter[] _parameters;
        private readonly CommandParameter _singleParameter;

        public CommandMethod(MethodInfo info)
        {
            _info = info;
            _parameters = _info.GetParameters().Select(p => new CommandParameter(p)).ToArray();

            if (_parameters.Length == 1)
                _singleParameter = _parameters.Single();
        }

        public bool HasParameters => _parameters.Length > 0;

        public bool CanExecute(object value)
        {
            if (value.IsAny(null, DependencyProperty.UnsetValue))
                return !HasParameters;

            if (_singleParameter != null)
                return _singleParameter.IsAssignable(value) == true;

            object[] values = value as object[];

            if (values?.Length != _parameters.Length)
                return false;

            return values.All((v, i) => _parameters[i].IsAssignable(v));
        }
        public object Execute(BaseCommand command, object value)
        {
            object[] values;

            if (HasParameters)
                values = value as object[] ?? new[] { value };
            else
                values = new object[0];

            return _info.Invoke(command, values);
        }
    }
}