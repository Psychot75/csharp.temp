using System.Windows.Input;

namespace Arcraven.Avalonia.ResourcesLib {

    public sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter)
            => _execute();

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null) return true;
            return _canExecute(Coerce(parameter));
        }

        public void Execute(object? parameter)
            => _execute(Coerce(parameter));

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        private static T Coerce(object? parameter)
        {
            if (parameter == null) return default!;

            if (parameter is T t) return t;

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            try
            {
                var converted = Convert.ChangeType(parameter, targetType);
                return (T)converted!;
            }
            catch
            {
                return default!;
            }
        }
    }
}