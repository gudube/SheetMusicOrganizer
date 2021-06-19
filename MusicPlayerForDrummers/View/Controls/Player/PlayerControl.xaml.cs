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

        private bool draggingVolume = false;


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!draggingVolume)
            {
                Settings.Default.Volume = (float)e.NewValue;
                Settings.Default.Save();
            }
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            this.draggingVolume = true;
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Slider? slider = sender as Slider;
            Settings.Default.Volume = slider is null ? 0f : (float)slider.Value;
            Settings.Default.Save();
            this.draggingVolume = false;
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
