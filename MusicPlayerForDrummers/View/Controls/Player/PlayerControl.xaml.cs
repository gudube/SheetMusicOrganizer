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
            if(playerVM.Session.PlayingSong != null)
            {
                string dir = Path.GetDirectoryName(playerVM.Session.PlayingSong.PartitionDirectory);
                Process.Start("explorer.exe", @dir);
            }
        }

        private void WaveformSeekBar_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
                return;
            playerVM.StartedSeekCommand.Execute(null);
        }

        private void WaveformSeekBar_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
                return;
            playerVM.StoppedSeekCommand.Execute(SeekBar.Value);
        }
    }
}
