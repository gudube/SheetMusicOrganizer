using System.ComponentModel;
using MusicPlayerForDrummers.Model.Tools;

//https://www.technical-recipes.com/2018/navigating-between-views-in-wpf-mvvm/
//https://csharpener.net/index.php/2018/08/21/th-evolution-of-the-implementation-in-inotifypropertychanged/
//https://stackoverflow.com/questions/1315621/implementing-inotifypropertychanged-does-a-better-way-exist
namespace MusicPlayerForDrummers.ViewModel
{
    public abstract class BaseViewModel : BaseNotifyPropertyChanged
    {
        public BaseViewModel(SessionContext session)
        {
            Session = session;
            Session.PropertyChanged += Session_PropertyChanged;
        }

        protected abstract void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e);

        public SessionContext Session { get; }

        public abstract string ViewModelName { get; }
    }
}
