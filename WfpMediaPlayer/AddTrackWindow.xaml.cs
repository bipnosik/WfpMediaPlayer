using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using WfpMediaPlayer.Data;
using WfpMediaPlayer.Models;
using NAudio.Wave;
using System.Windows.Controls;

namespace WfpMediaPlayer.Views
{
    public partial class AddTrackWindow : Window
    {
        private readonly DatabaseHelper dbHelper;
        public WfpMediaPlayer.Models.Track NewTrack { get; private set; } // Явно указано пространство имён

        public AddTrackWindow()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadArtists();
            LoadAlbums();
        }

        private void LoadArtists()
        {
            List<Artist> artists = dbHelper.GetArtists();
            ArtistComboBox.ItemsSource = artists;
        }

        private void LoadAlbums()
        {
            List<Album> albums = dbHelper.GetAlbums();
            AlbumComboBox.ItemsSource = albums;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Медиа файлы (*.mp3;*.mp4)|*.mp3;*.mp4|Все файлы (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
                TitleTextBox.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FilePathTextBox.Text))
            {
                MessageBox.Show("Выберите файл трека!");
                return;
            }

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название трека!");
                return;
            }

            if (ArtistComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите исполнителя!");
                return;
            }

            string filePath = FilePathTextBox.Text;
            TimeSpan? duration = GetTrackDuration(filePath);

            NewTrack = new WfpMediaPlayer.Models.Track // Явно указано пространство имён
            {
                Title = TitleTextBox.Text,
                ArtistID = ((Artist)ArtistComboBox.SelectedItem).ArtistID,
                AlbumID = AlbumComboBox.SelectedItem != null ? ((Album)AlbumComboBox.SelectedItem).AlbumID : (int?)null,
                Duration = duration,
                Genre = ((ComboBoxItem)GenreComboBox.SelectedItem).Content.ToString(),
                FilePath = filePath,
                Format = Path.GetExtension(filePath).TrimStart('.')
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static TimeSpan? GetTrackDuration(string filePath)
        {
            try
            {
                using (var reader = new MediaFoundationReader(filePath))
                {
                    return reader.TotalTime;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка определения длительности трека: " + ex.Message);
                return null;
            }
        }
    }
}