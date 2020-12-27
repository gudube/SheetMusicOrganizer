using System;
using System.Windows;
using Microsoft.Win32;
using MusicPlayerForDrummers.Model.Items;
using MusicPlayerForDrummers.ViewModel;
using Serilog;

namespace MusicPlayerForDrummers.View.Windows
{
    /// <summary>
    /// Interaction logic for AddNewSongWindow.xaml
    /// </summary>
    public partial class AddNewSongWindow : Window
    {
        public AddNewSongWindow()
        {
            this.Owner = Application.Current.MainWindow;
            Song = new SongItem();
            InitializeComponent();
            ResetSongInformations();
        }

        #region File Selection
        public static readonly DependencyProperty SongProperty = DependencyProperty.Register("Song", typeof(SongItem), typeof(AddNewSongWindow));
        public SongItem Song { get => (SongItem)GetValue(SongProperty); set => SetValue(SongProperty, value); }

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

        private void SelectAudioFile1Button_Click(object sender, RoutedEventArgs e)
        {
            SelectAudioFile(true);
        }

        private void SelectAudioFile2Button_Click(object sender, RoutedEventArgs e)
        {
            SelectAudioFile(false);
        }

        private void SelectAudioFile(bool mainAudio)
        {
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = "Audio (*.mp3, *.flac)|*.mp3;*.flac|All Files (*.*)|*.*",
                Multiselect = false,
                FilterIndex = 1
            };
            if (openDialog.ShowDialog() == true)
            {
                if (mainAudio)
                    Song.AudioDirectory1 = openDialog.FileName;
                else
                    Song.AudioDirectory2 = openDialog.FileName;
                Song.ReadAudioMetadata();
                if(UseAudioMDCheckBox.IsChecked == true)
                    SetSongInformations();
            }
        }

        private void UseAudioMD_Checked(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(Song.AudioDirectory1))
                SetSongInformations();
        }
        private void UseAudioMD_Unchecked(object sender, RoutedEventArgs e)
        {
            ResetSongInformations();
        }
        #endregion

        #region Song information
        public static readonly DependencyProperty NumberTextProperty = DependencyProperty.Register("NumberText", typeof(uint), typeof(AddNewSongWindow));
        public uint NumberText { get => (uint)GetValue(NumberTextProperty); set => SetValue(NumberTextProperty, value); }

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

        private uint _numberBackup = 0;
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
            if(!(Owner.DataContext is MainVM mainVM))
                return;

            if (!string.IsNullOrWhiteSpace(Song.PartitionDirectory) && !string.IsNullOrWhiteSpace(TitleText))
            {
                UpdateSongFromInformation();

                if (mainVM.AddSong(Song))
                {
                    this.Close();
                }
                else
                {
                    string message = "This music sheet already exists in the library.\nWould you like to go to the song?";
                    GenericWindow songExistingWindow = new GenericWindow(this, message, "Go To Song");
                    if (songExistingWindow.DialogResult.HasValue && songExistingWindow.DialogResult.Value)
                    {
                        try
                        {
                            mainVM.GoToSong(Song.PartitionDirectory);
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            string error = $"Could not find the music sheet {Song.PartitionDirectory} in the library.";
                            Log.Error("Error: {error} Exception message: {message}", error, ex.Message);
                            ErrorWindow unused = new ErrorWindow(this, error);
                        }
                    }
                }
            }
        }

        private void UpdateSongFromInformation()
        {
            Song.Number = NumberText;
            Song.Title = TitleText;
            Song.Artist = ArtistText;
            Song.Album = AlbumText;
            Song.Genre = GenreText;
            Song.Rating = RatingText;
            if (string.IsNullOrWhiteSpace(Song.AudioDirectory1) && !string.IsNullOrWhiteSpace(Song.AudioDirectory2))
            {
                Song.AudioDirectory1 = Song.AudioDirectory2;
                Song.AudioDirectory2 = "";
            }
        }
    }
}
