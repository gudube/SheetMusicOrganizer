#Technology used: sqlite, MVVM, C#, WPF

##Need to bind to a dynamic property:
only in the view? See SwitchViewButton.xaml.cs (DependencyProperty)
	https://stackoverflow.com/questions/25895011/how-to-add-custom-properties-to-wpf-user-control
also in model/viewmodel? see MainVM.cs (INotifyPropertyChanged, SetValue) or PlaylistItem.cs
Event from view to viewmodel? See MainVM.cs (DelegateCommand)

##set Datacontext: see MainWindow.xaml

##Logging
Using Serilog.Log... Implemented in MainWindow.xaml.cs
Should be able to see it from the visual studio debug window.

##Songs
###Order
Each PlaylistSongItem will remember its position in the playlist.
When deleting a song or playlist, the foreign key should cascade and the PlaylistSongItem should delete.

##Playlists
We suppose the playlist[0] is the locked 'All Music' playlist. It should never be null or another playlist.

##Status
Every status is associated to a boolean in StatusContext.cs (instance kept in Session).
Set the correct boolean true right before starting an operation.
Set the boolean to false as soon as it is done (most of the time, not the same place as when set true)
If need a new status, add a boolean with its message in the setter (see already existing ones)