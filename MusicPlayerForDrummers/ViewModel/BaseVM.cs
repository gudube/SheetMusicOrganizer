using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

//https://www.technical-recipes.com/2018/navigating-between-views-in-wpf-mvvm/
//https://csharpener.net/index.php/2018/08/21/th-evolution-of-the-implementation-in-inotifypropertychanged/
//https://stackoverflow.com/questions/1315621/implementing-inotifypropertychanged-does-a-better-way-exist
namespace MusicPlayerForDrummers.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void SetField<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;
            OnPropertyChanged(propertyName);
        }
        #endregion

        public abstract string ViewModelName { get; }
    }
}
