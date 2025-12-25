using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arcraven.Avalonia.ResourcesLib
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null) return;
            for (int i = 0; i < propertyNames.Length; i++)
                OnPropertyChanged(propertyNames[i]);
        }
    }
}