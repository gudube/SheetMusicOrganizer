# Technology used: sqlite, MVVM, C#, WPF

## Need to bind to a dynamic property:
only in the view? See SwitchViewButton.xaml.cs (DependencyProperty)
	https://stackoverflow.com/questions/25895011/how-to-add-custom-properties-to-wpf-user-control
also in model/viewmodel? see MainVM.cs (INotifyPropertyChanged, SetValue) or PlaylistItem.cs
Event from view to viewmodel? See MainVM.cs (DelegateCommand)

## set Datacontext: see MainWindow.xaml

## Logging
Using Serilog.Log... Implemented in MainWindow.xaml.cs
Should be able to see it from the visual studio debug window.

## Songs
### Order
Each PlaylistSongItem will remember its position in the playlist.
When deleting a song or playlist, the foreign key should cascade and the PlaylistSongItem should delete.

## Playlists
We suppose the playlist[0] is the locked 'All Music' playlist. It should never be null or another playlist.

## Status
Every status is associated to a boolean in **StatusContext.cs** (instance kept in Session).
Call addLoadingStatus and removeLoadingStatus when done.
Can add new statuses in the enum LoadingStatus and update getMessage()

## Errors
### Error window
Call GlobalEvents.raiseErrorEvent with the exception (with complex message) and a user friendly message that will be displayed (optional).
Automatically done for any Execute in DelegateCommand.
In advanced section, the exception message is always used. In the user friendly message, the custom message is used if passed by parameter.
Otherwise, look into the generic exceptions message/title. Otherwise, stays empty and generic title. Try to add more custom exception types if possible.
### Error Log
If the error could be hidden to the user, use: Log.Error("Trying to sort by column {column} when the dataContext is not libraryVM but is {dataContext}", column.Header, DataContext?.GetType());
from Serilog

# Deployment
dotnet publish -c Release -r win-x64 --output ./bin/SheetMusicOrganizer_v1