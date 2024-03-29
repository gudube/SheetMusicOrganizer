﻿using System;
using System.Windows;
using Microsoft.Win32;
using SheetMusicOrganizer.View.Tools;
using Serilog;
using SheetMusicOrganizer.Model.Items;
using SheetMusicOrganizer.ViewModel;
using SheetMusicOrganizer.ViewModel.Library;

namespace SheetMusicOrganizer.View.Windows
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
            this.WindowStyle = WindowStyle.ToolWindow;
            this.ResizeMode = ResizeMode.NoResize;
            Loaded += AddNewSongWindow_Loaded;
            InitializeComponent();
            ResetSongInformations();
        }

        private void AddNewSongWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.WidthAndHeight;
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
            var audioStrings = string.Join(", ", ImportLibraryVM.SupportedFormats.Select(x => $"*{x}"));
            var audioExtensions = string.Join(";", ImportLibraryVM.SupportedFormats.Select(x => $"*{x}"));
            OpenFileDialog openDialog = new OpenFileDialog
            {
                Filter = $"Audio ({audioStrings})|{audioExtensions}|All Files (*.*)|*.*",
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

        public static readonly DependencyProperty YearTextProperty = DependencyProperty.Register("YearText", typeof(uint?), typeof(AddNewSongWindow));
        public uint? YearText { get => (uint?)GetValue(YearTextProperty); set => SetValue(YearTextProperty, value); }

        public static readonly DependencyProperty RatingTextProperty = DependencyProperty.Register("RatingText", typeof(uint), typeof(AddNewSongWindow));
        public uint RatingText { get => (uint)GetValue(RatingTextProperty); set => SetValue(RatingTextProperty, value); }

        private uint _numberBackup = 0;
        private string _titleBackup = "";
        private string _artistBackup = "";
        private string _albumBackup = "";
        private string _genreBackup = "";
        private uint? _yearBackup = null;
        private uint _ratingBackup = 0;

        private void ResetSongInformations()
        {
            NumberText = _numberBackup;
            TitleText = _titleBackup;
            ArtistText = _artistBackup;
            AlbumText = _albumBackup;
            GenreText = _genreBackup;
            YearText = _yearBackup;
            RatingText = _ratingBackup;
        }

        private void SetSongInformations()
        {
            _numberBackup = NumberText;
            _titleBackup = TitleText;
            _artistBackup = ArtistText;
            _albumBackup = AlbumText;
            _genreBackup = GenreText;
            _yearBackup = YearText;
            _ratingBackup = RatingText;

            NumberText = Song.Number;
            TitleText = Song.Title;
            ArtistText = Song.Artist;
            AlbumText = Song.Album;
            GenreText = Song.Genre;
            YearText = Convert.ToUInt32(Song.Year);
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

                var importVM = new ImportLibraryVM(mainVM.Session, mainVM.LibraryVM);
                if (importVM.AddSong(Song, false))
                {
                    mainVM.GoToSong(Song, false);
                    this.Close();
                }
                else
                {
                    string message = "This music sheet already exists in the library.\nWould you like to overwrite the song? (warning: resets sync configuration)";
                    GenericWindow songExistingWindow = new GenericWindow(this, message, "Overwrite");
                    if (songExistingWindow.DialogResult.HasValue && songExistingWindow.DialogResult.Value)
                    {
                        try
                        {
                            importVM.AddSong(Song, true);
                            mainVM.GoToSong(Song, false);
                            Application.Current.Dispatcher.InvokeAsync(() => { this.Close(); },  System.Windows.Threading.DispatcherPriority.ContextIdle);
                        }
                        catch (Exception ex)
                        {
                            GlobalEvents.raiseErrorEvent(ex, $"Could not find the music sheet {Song.PartitionDirectory} in the library.");
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
            Song.Year = YearText?.ToString() ?? "";
            Song.Rating = RatingText;
            if (string.IsNullOrWhiteSpace(Song.AudioDirectory1) && !string.IsNullOrWhiteSpace(Song.AudioDirectory2))
            {
                Song.AudioDirectory1 = Song.AudioDirectory2;
                Song.AudioDirectory2 = "";
            }
        }
    }
}
