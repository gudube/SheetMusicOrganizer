using Microsoft.Win32;
using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayerForDrummers.View
{
    /// <summary>
    /// Interaction logic for SongsGrid.xaml
    /// </summary>
    public partial class SongsGrid : UserControl
    {
        public SongsGrid()
        {
            InitializeComponent();
        }

        //TODO: Accept drag-and-drop
        //TODO: Hide button after a song is added
        //TODO: Add this option in File->Add Song
        //TODO: Directory import (batch import, see performance with taglib)
        //TODO: Test more formats
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Audio (*.mp3, *.flac)|*.mp3;*.flac|All Files (*.*)|*.*",
                Multiselect = true,
                FilterIndex = 1
            };
            if (openDialog.ShowDialog() == true)
            {
                foreach (string fileName in openDialog.FileNames)
                    ((LibraryVM)this.DataContext).AddSongFileCommand.Execute(fileName);
            }
        }

        private void OpenDirButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
