using Microsoft.Win32;
using MusicPlayerForDrummers.Model;
using MusicPlayerForDrummers.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TagLib.Mpeg;

namespace MusicPlayerForDrummers.View
{
    /// <summary>
    /// Interaction logic for AddNewSongWindow.xaml
    /// </summary>
    public partial class AddNewSongWindow : Window
    {
        //TODO: Rajouter bouton pour enlever le champ audio file
        //TODO: Changer couleur texte textbox
        //TODO: Ouvrir fenetre si chanson existe deja
        public AddNewSongWindow()
        {
            this.Owner = App.Current.MainWindow;
            this.DataContext = this;
            Song = new SongItem();
            InitializeComponent();
            ResetSongInformations();
        }

        #region File Selection
        public static readonly DependencyProperty SongProperty = DependencyProperty.Register("Song", typeof(SongItem), typeof(AddNewSongWindow));
        public SongItem Song { get => (SongItem)GetValue(SongProperty); set => SetValue(SongProperty, value); }

        //public static readonly DependencyProperty PartitionFilenameProperty = DependencyProperty.Register("PartitionFilename", typeof(string), typeof(AddNewSongWindow));
        //public string PartitionFilename { get => (string)GetValue(PartitionFilenameProperty); set => SetValue(PartitionFilenameProperty, value); }

        //public static readonly DependencyProperty AudioFilenameProperty = DependencyProperty.Register("AudioFilename", typeof(string), typeof(AddNewSongWindow));
        //public string AudioFilename { get => (string)GetValue(AudioFilenameProperty); set => SetValue(AudioFilenameProperty, value); }

        private void SelectMusicSheetButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Music Sheet (*.pdf)|*.pdf;|All Files (*.*)|*.*",
                Multiselect = false,
                FilterIndex = 1
            };
            if (openDialog.ShowDialog() == true)
            {
                Song.PartitionDirectory = openDialog.FileName;
            }
        }

        private void SelectAudioFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Audio (*.mp3, *.flac)|*.mp3;*.flac|All Files (*.*)|*.*",
                Multiselect = false,
                FilterIndex = 1
            };
            if (openDialog.ShowDialog() == true)
            {
                Song.AudioDirectory = openDialog.FileName;
                Song.ReadAudioMetadata();
                if(UseAudioMDCheckBox.IsChecked == true)
                    SetSongInformations();
            }
        }

        private void UseAudioMD_Checked(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(Song.AudioDirectory))
                SetSongInformations();
        }
        private void UseAudioMD_Unchecked(object sender, RoutedEventArgs e)
        {
            ResetSongInformations();
        }
        #endregion

        #region Song information
        public static readonly DependencyProperty NumberTextProperty = DependencyProperty.Register("NumberText", typeof(uint?), typeof(AddNewSongWindow));
        public uint? NumberText { get => (uint?)GetValue(NumberTextProperty); set => SetValue(NumberTextProperty, value); }

        public static readonly DependencyProperty TitleTextProperty = DependencyProperty.Register("TitleText", typeof(string), typeof(AddNewSongWindow));
        public string TitleText { get => (string)GetValue(TitleTextProperty); set => SetValue(TitleTextProperty, value); }

        public static readonly DependencyProperty ArtistTextProperty = DependencyProperty.Register("ArtistText", typeof(string), typeof(AddNewSongWindow));
        public string ArtistText { get => (string)GetValue(ArtistTextProperty); set => SetValue(ArtistTextProperty, value); }

        public static readonly DependencyProperty AlbumTextProperty = DependencyProperty.Register("AlbumText", typeof(string), typeof(AddNewSongWindow));
        public string AlbumText { get => (string)GetValue(AlbumTextProperty); set => SetValue(AlbumTextProperty, value); }

        public static readonly DependencyProperty GenreTextProperty = DependencyProperty.Register("GenreText", typeof(string), typeof(AddNewSongWindow));
        public string GenreText { get => (string)GetValue(GenreTextProperty); set => SetValue(GenreTextProperty, value); }

        public static readonly DependencyProperty RatingTextProperty = DependencyProperty.Register("RatingText", typeof(uint), typeof(AddNewSongWindow));
        public uint RatingText { get => (uint)GetValue(RatingTextProperty); set => SetValue(RatingTextProperty, value); }

        private uint? _numberBackup = 0;
        private string _titleBackup = "";
        private string _artistBackup = "";
        private string _albumBackup = "";
        private string _genreBackup = "";
        private uint _ratingBackup = 0;

        private void ResetSongInformations()
        {
            NumberText = _numberBackup;
            TitleText = _titleBackup;
            ArtistText = _artistBackup;
            AlbumText = _albumBackup;
            GenreText = _genreBackup;
            RatingText = _ratingBackup;
        }

        private void SetSongInformations()
        {
            _numberBackup = NumberText;
            _titleBackup = TitleText;
            _artistBackup = ArtistText;
            _albumBackup = AlbumText;
            _genreBackup = GenreText;
            _ratingBackup = RatingText;

            NumberText = Song.Number;
            TitleText = Song.Title;
            ArtistText = Song.Artist;
            AlbumText = Song.Album;
            GenreText = Song.Genre;
            RatingText = Song.Rating;
        }
        #endregion

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            MainVM mainVM = Owner.DataContext as MainVM;
            if (!string.IsNullOrWhiteSpace(Song.PartitionDirectory) && !string.IsNullOrWhiteSpace(TitleText))
            {
                if (mainVM.IsSongExisting(Song.PartitionDirectory))
                {
                    string message = "This music sheet already exists in the library.\nWould you like to go to the song?";
                    GenericWindow songExistingWindow = new GenericWindow(message, "Go To Song");
                    songExistingWindow.Owner = this;
                    songExistingWindow.ShowDialog();
                    if (songExistingWindow.DialogResult.HasValue && songExistingWindow.DialogResult.Value)
                    {
                        mainVM.GoToSong(Song.PartitionDirectory);
                        this.Close();
                    }
                }
                else
                {
                    UpdateSongFromInformations();
                    mainVM.AddNewSong(Song);
                    this.Close();
                }
            }
        }

        private void UpdateSongFromInformations()
        {
            Song.Number = NumberText;
            Song.Title = TitleText;
            Song.Artist = ArtistText;
            Song.Album = AlbumText;
            Song.Genre = GenreText;
            Song.Rating = RatingText;
        }
    }
}
