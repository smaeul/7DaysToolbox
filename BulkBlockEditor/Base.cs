namespace BulkBlockEditor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    class RelayCommand : ICommand
    {
        private readonly Action<Object> _action;
        private readonly Predicate<Object> _canExecute;

        public RelayCommand(Action<Object> action) : this(action, null)
        {
            // No additional implementation.
        }

        public RelayCommand(Action<Object> action, Predicate<Object> canExecute)
        {
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }
            this._action = action;
            this._canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public Boolean CanExecute(Object parameter)
        {
            return this._canExecute == null ? true : this._canExecute(parameter);
        }

        public void Execute(Object parameter)
        {
            this._action(parameter);
        }
    }

    class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] String propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected Boolean SetField<U>(ref U field, U value, [CallerMemberName] String propertyName = null)
        {
            if (EqualityComparer<U>.Default.Equals(field, value)) {
                return false;
            }
            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    sealed class CallerMemberNameAttribute : Attribute { }
}
