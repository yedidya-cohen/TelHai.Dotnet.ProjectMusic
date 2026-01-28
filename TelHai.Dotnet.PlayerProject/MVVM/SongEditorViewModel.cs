using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TelHai.Dotnet.PlayerProject.Models;

namespace TelHai.Dotnet.PlayerProject.MVVM
{
    public class SongEditorViewModel : ViewModelBase
    {
        private readonly MusicTrack _originalTrack;

        // --- 1. Temporary Properties for Editing ---
        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string _artist;
        public string Artist
        {
            get => _artist;
            set { _artist = value; OnPropertyChanged(); }
        }

        private string _album;
        public string Album
        {
            get => _album;
            set { _album = value; OnPropertyChanged(); }
        }

        // --- 2. Image Management ---

        // The text box where user pastes a new URL
        private string _newImageUrl = "";
        public string NewImageUrl
        {
            get => _newImageUrl;
            set { _newImageUrl = value; OnPropertyChanged(); }
        }

        // The list shown in the UI (Auto-updates)
        public ObservableCollection<string> Images { get; set; }

        // Which image is currently selected in the list (for removal)
        private string? _selectedImage;
        public string? SelectedImage
        {
            get => _selectedImage;
            set { _selectedImage = value; OnPropertyChanged(); }
        }

        // --- 3. Commands (Buttons) ---
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand RemoveImageCommand { get; }

        // Event to tell the Window to close (View-Model communication)
        public event Action<bool>? CloseRequested;

        // --- Constructor ---
        public SongEditorViewModel(MusicTrack track)
        {
            _originalTrack = track;

            // Copy data to temporary properties 
            Title = track.Title;
            Artist = track.Artist ?? "";
            Album = track.Album ?? "";
            Images = new ObservableCollection<string>(track.Images);

            // Setup Buttons
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => CloseRequested?.Invoke(false));

            // Add Image only if text is not empty
            AddImageCommand = new RelayCommand(_ => AddImage(), _ => !string.IsNullOrWhiteSpace(NewImageUrl));

            // Remove Image only if one is selected
            RemoveImageCommand = new RelayCommand(_ => RemoveImage(), _ => SelectedImage != null);
        }

        // --- Logic Methods ---

        private void Save()
        {
            // Commit changes back to the original object
            _originalTrack.Title = Title;
            _originalTrack.Artist = Artist;
            _originalTrack.Album = Album;

            // Update the list of images
            _originalTrack.Images.Clear();
            foreach (var img in Images) _originalTrack.Images.Add(img);

            CloseRequested?.Invoke(true); // Signal success
        }

        private void AddImage()
        {
            if (!string.IsNullOrWhiteSpace(NewImageUrl) && !Images.Contains(NewImageUrl))
            {
                Images.Add(NewImageUrl);
                NewImageUrl = string.Empty; // Clear the text box
            }
        }

        private void RemoveImage()
        {
            if (SelectedImage != null)
            {
                Images.Remove(SelectedImage);
            }
        }
    }
}
