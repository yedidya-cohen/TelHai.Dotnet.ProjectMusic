using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TelHai.Dotnet.PlayerProject.Models;
using TelHai.Dotnet.PlayerProject.Services;

namespace TelHai.Dotnet.PlayerProject
{
    /// <summary>
    /// Interaction logic for MusicPlayer.xaml
    /// </summary>
    public partial class MusicPlayer : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        private List<MusicTrack> library = new List<MusicTrack>();
        private bool isDragging = false;
        private const string FILE_NAME = "library.json";
        private readonly ItunesService _itunes = new ItunesService();
        private CancellationTokenSource? _cts;

        public MusicPlayer()
        {
            //--init all Hardcoded xmal into Elemnts Tree
            InitializeComponent();
            
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(Timer_Tick);
            this.Loaded += MusicPlayer_Loaded;
        }

        private void MusicPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLibrary();
        }


        // --- EMPTY PLACEHOLDERS TO MAKE IT BUILD ---
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is not MusicTrack track)
            {
                txtStatus.Text = "No track selected.";
                return;
            }
            this.StartTrack(track);
        }
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            txtStatus.Text = "Paused";
        }
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            sliderProgress.Value = 0;
            txtStatus.Text = "Stopped";
        }
        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sliderVolume.Value;
        }
        private void Slider_DragStarted(object sender, MouseButtonEventArgs e)
        {
            isDragging = true; // Stop timer updates
        }

        private void Slider_DragCompleted(object sender, MouseButtonEventArgs e)
        {
            isDragging = false; // Resume timer updates
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }


        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            //file dialog to choose file from system
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "MP3 Files|*.mp3";
            //user confirmd
            if (ofd.ShowDialog() == true)
            {
                //iterate all files selected as strings
                foreach (string file in ofd.FileNames)
                {
                    //create object for each file
                    MusicTrack track = new MusicTrack
                    {
                        //only File name 
                        Title = System.IO.Path.GetFileNameWithoutExtension(file),
                        //full path
                        FilePath = file
                    };
                    library.Add(track);
                }
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                library.Remove(track);
                UpdateLibraryUI();
                SaveLibrary();
            }
        }


        // Helper method to refresh the box
        private void UpdateLibraryUI()
        {
            //take all liabary list as source to the list box
            //display toString for inner object within list
            lstLibrary.ItemsSource = null;
            lstLibrary.ItemsSource = library;
        }
        private void SaveLibrary()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(library,options);
            File.WriteAllText(FILE_NAME, json);
        }
        private void LoadLibrary()
        {
            if (File.Exists(FILE_NAME)) 
            {
                //read file
                string json = File.ReadAllText(FILE_NAME);
                //create list of musicTrack from json
                library = JsonSerializer.Deserialize<List<MusicTrack>>(json) ?? new List<MusicTrack>();
                
                UpdateLibraryUI();
            }
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Update slider ONLY if music is loaded AND user is NOT holding the handle
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && !isDragging)
            {
                sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliderProgress.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            //1) create setting window Instance
            Settings settingsWin = new Settings();

            //2) subscribe/register to OnScanCompleted Event
            // Listen for the results
            settingsWin.OnScanCompleted += SettingsWin_OnScanCompleted;
            settingsWin.ShowDialog();

            
        }

        private void SettingsWin_OnScanCompleted(List<MusicTrack> newTracksEventData)
        {
            foreach (var track in newTracksEventData)
            {
                // Prevent duplicates based on FilePath
                if (!library.Any(x => x.FilePath == track.FilePath))
                {
                    library.Add(track);
                }
            }

            UpdateLibraryUI();
            SaveLibrary();
        }

        private void lstLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLibrary.SelectedItem is not MusicTrack track) return;

            txtCurrentSong.Text = track.Title;   
            txtFilePath.Text = track.FilePath;
            txtSongName.Text = "-";
            txtArtistName.Text = "-";
            txtAlbumName.Text = "-";
            txtPath.Text = "-";
            SetDefaultArtwork();

        }
        private void LstLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                this.StartTrack(track);
            }
        }
        private void StartTrack(MusicTrack track)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            mediaPlayer.Stop();
            mediaPlayer.Open(new Uri(track.FilePath));
            mediaPlayer.Play();
            sliderProgress.Value = 0;
            timer.Start();

            txtCurrentSong.Text = track.Title;
            txtStatus.Text = "Playing";
            txtFilePath.Text = track.FilePath;
            txtSongName.Text = "-";
            txtArtistName.Text = "-";
            txtAlbumName.Text = "-";
            SetDefaultArtwork();
            _ = LoadSongInfoAsync(track, token);
        }
        private void SetDefaultArtwork()
        {
            imgArtwork.Source = new BitmapImage(new Uri("C:\\Users\\yedid\\Third Year\\C#\\" +
                                "TelHai.Dotnet.PlayerProject\\TelHai.Dotnet.PlayerProject\\Assets\\placeholder.png"));
        }
        private async Task LoadSongInfoAsync(MusicTrack track, CancellationToken token)
        {
            try
            {
                // Assumption: Title is the artist term or song name
                var query = track.Title;

                var info = await _itunes.SearchOneAsync(query, token);

                if (token.IsCancellationRequested)
                    return;

                if (info == null)
                {
                    // no results: keep local title+path and default image
                    return;
                }

                // Update UI with iTunes data
                txtSongName.Text = string.IsNullOrWhiteSpace(info.TrackName)
                    ? track.Title
                    : info.TrackName;

                txtArtistName.Text = string.IsNullOrWhiteSpace(info.ArtistName) ? "-" : info.ArtistName;
                txtAlbumName.Text = string.IsNullOrWhiteSpace(info.AlbumName) ? "-" : info.AlbumName;
                txtPath.Text = track.FilePath;

                if (!string.IsNullOrWhiteSpace(info.ArtworkUrl))
                    UpdateArtworkFromUrl(info.ArtworkUrl);
                else
                    SetDefaultArtwork();
            }
            catch (OperationCanceledException)
            {
                // expected when switching tracks quickly
            }
            catch
            {
                // requirement: on error show filename without extension + local path
                txtSongName.Text = System.IO.Path.GetFileNameWithoutExtension(track.FilePath);
                txtPath.Text = track.FilePath;

                txtArtistName.Text = "-";
                txtAlbumName.Text = "-";
                SetDefaultArtwork();
            }
        }
        private void UpdateArtworkFromUrl(string url)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(url);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                imgArtwork.Source = bmp;
            }
            catch
            {
                SetDefaultArtwork();
            }
        }
    }
}
