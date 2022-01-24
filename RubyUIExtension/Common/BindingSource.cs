using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RubyUIExtension.Common
{
    public abstract class BindingSource : INotifyPropertyChanged
    {
        public void SetValue<T>(ref T source, T value, [CallerMemberName] string prop = "")
        {
            if (source?.Equals(value) == true)
                return;

            source = value;

            OnPropertyChanged(prop);
        }

        public void UpdateValue([CallerMemberName] string prop = "")
        {
            OnPropertyChanged(prop);
        }

        public abstract void UpdateAllValue();

        #region Implementation INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #endregion
    }
}
