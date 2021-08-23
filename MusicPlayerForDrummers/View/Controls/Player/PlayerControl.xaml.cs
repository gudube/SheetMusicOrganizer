using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using SheetMusicOrganizer.ViewModel;

namespace SheetMusicOrganizer.View.Controls.Player
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
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

        /*private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is PlayerVM playerVM))
                return;
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }*/
    }
}
