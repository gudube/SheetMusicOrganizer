using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace MusicPlayerForDrummers.View
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
    }
}
