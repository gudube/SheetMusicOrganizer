using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MusicPlayerForDrummers.ViewModel;

namespace MusicPlayerForDrummers.View.Controls.Player
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    /// TODO: Add button (on the right of the player?) to remove the waveform part (only keep buttons)
    /// on top of the LibraryMenu or PartitionMenu, leaving more place for the rest 
    public partial class PlayerControl : UserControl
    {
        public PlayerControl()
        {
            InitializeComponent();
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
                return;
            if (playerVM.Session.PlayingSong == null)
                return;

            string? dir = Path.GetDirectoryName(playerVM.Session.PlayingSong.PartitionDirectory);
            if(dir != null)
                Process.Start("explorer.exe", dir);
        }
    }
}
