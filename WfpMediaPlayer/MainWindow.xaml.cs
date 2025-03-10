using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Microsoft.Win32;
using WfpMediaPlayer.Data;
using WfpMediaPlayer.Models;
using System.IO;
using NAudio.Wave;
using System.Windows.Media;

namespace WfpMediaPlayer.Views
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseHelper dbHelper = new DatabaseHelper();
        private List<WfpMediaPlayer.Models.Track> tracks;
        private List<Playlist> playlists;
        private List<Artist> artists;
        private List<Album> albums;
        private readonly Models.User currentUser;
        private readonly DispatcherTimer updateTimer;
        private bool isDragging = false;
        private TimeSpan trackDuration;

        public MainWindow(Models.User user)
        {
            InitializeComponent();
            currentUser = user ?? throw new ArgumentNullException("user");
            tracks = new List<WfpMediaPlayer.Models.Track>();
            playlists = new List<Playlist>();
            artists = new List<Artist>();
            albums = new List<Album>();

            LoadArtists();
            LoadAlbums();
            LoadPlaylists();
            LoadTracksForPlaylist(null);

            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(500);
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void LoadTracksForPlaylist(int? playlistId)
        {
            if (playlistId.HasValue)
            {
                tracks = dbHelper.GetTracksByPlaylist(playlistId.Value);
            }
            else
            {
                tracks = dbHelper.GetTracks();
            }
            TracksListView.ItemsSource = tracks;
        }

        private void LoadPlaylists()
        {
            playlists = dbHelper.GetPlaylists(currentUser.UserID);
            PlaylistsComboBox.ItemsSource = playlists;
            PlaylistsComboBox.DisplayMemberPath = "Name";
        }

        private void LoadArtists()
        {
            artists = dbHelper.GetArtists();
            ArtistsComboBox.ItemsSource = artists;
        }

        private void LoadAlbums()
        {
            albums = dbHelper.GetAlbums();
            AlbumsComboBox.ItemsSource = albums;
        }

        // Новая функция поиска
        private void SearchTracks(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                // Если запрос пустой, загружаем все треки текущего плейлиста или все треки
                if (PlaylistsComboBox.SelectedItem is Playlist selectedPlaylist)
                {
                    LoadTracksForPlaylist(selectedPlaylist.PlaylistID);
                }
                else
                {
                    LoadTracksForPlaylist(null);
                }
            }
            else
            {
                // Ищем треки по названию
                tracks = dbHelper.SearchTracksByTitle(searchQuery);
                TracksListView.ItemsSource = tracks;
            }
        }

        // Обработчик нажатия кнопки поиска
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text.Trim();
            SearchTracks(searchQuery);
        }

        // Опционально: поиск в реальном времени при вводе текста
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text.Trim();
            SearchTracks(searchQuery);
        }

        private void TracksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TracksListView.SelectedItem is WfpMediaPlayer.Models.Track selectedTrack)
            {
                try
                {
                    string filePath = selectedTrack.FilePath;
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show($"Файл не найден: {filePath}");
                        return;
                    }

                    trackDuration = GetTrackDuration(filePath);
                    if (trackDuration != TimeSpan.Zero)
                    {
                        ProgressSlider.Maximum = trackDuration.TotalSeconds;
                        TotalTimeDisplay.Text = trackDuration.ToString(@"mm\:ss");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось определить длительность трека.");
                    }

                    MediaPlayer.Source = new Uri(filePath, UriKind.Absolute);
                    MediaPlayer.Play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке трека: " + ex.Message);
                }
            }
        }

        private static TimeSpan GetTrackDuration(string filePath)
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
                MessageBox.Show("Ошибка NAudio: " + ex.Message);
                return TimeSpan.Zero;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Source != null)
            {
                MediaPlayer.Play();
            }
            else
            {
                MessageBox.Show("Трек не выбран!");
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            ProgressSlider.Value = 0;
            CurrentTimeDisplay.Text = "00:00";
        }

        private void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Source != null)
            {
                double pos = MediaPlayer.Position.TotalSeconds - 10;
                MediaPlayer.Position = TimeSpan.FromSeconds(pos < 0 ? 0 : pos);
                UpdateSliderAndTime();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Source != null)
            {
                double pos = MediaPlayer.Position.TotalSeconds + 10;
                if (trackDuration != TimeSpan.Zero && pos > trackDuration.TotalSeconds)
                {
                    pos = trackDuration.TotalSeconds;
                }
                MediaPlayer.Position = TimeSpan.FromSeconds(pos);
                UpdateSliderAndTime();
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (MediaPlayer.Source != null && !isDragging)
            {
                UpdateSliderAndTime();
            }
        }

        private void UpdateSliderAndTime()
        {
            double pos = MediaPlayer.Position.TotalSeconds;
            ProgressSlider.Value = pos;
            CurrentTimeDisplay.Text = TimeSpan.FromSeconds(pos).ToString(@"mm\:ss");
            if (trackDuration != TimeSpan.Zero)
            {
                ProgressSlider.Maximum = trackDuration.TotalSeconds;
                TotalTimeDisplay.Text = trackDuration.ToString(@"mm\:ss");
            }
            else
            {
                ProgressSlider.Maximum = 300;
                TotalTimeDisplay.Text = "??:??";
            }
        }

        private void ProgressSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void ProgressSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isDragging = false;
            if (MediaPlayer.Source != null)
            {
                double pos = ProgressSlider.Value;
                if (trackDuration != TimeSpan.Zero && pos > trackDuration.TotalSeconds)
                {
                    pos = trackDuration.TotalSeconds;
                }
                MediaPlayer.Position = TimeSpan.FromSeconds(pos);
                UpdateSliderAndTime();
            }
        }

        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Медиа файлы (*.mp4)|*.mp4|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string title = System.IO.Path.GetFileNameWithoutExtension(filePath);

                WfpMediaPlayer.Models.Track newTrack = new WfpMediaPlayer.Models.Track
                {
                    Title = title,
                    ArtistID = ArtistsComboBox.SelectedItem is Artist artist ? artist.ArtistID : 1,
                    AlbumID = AlbumsComboBox.SelectedItem is Album album ? album.AlbumID : (int?)null,
                    Duration = GetTrackDuration(filePath),
                    Genre = "Unknown",
                    FilePath = filePath,
                    Format = "mp4"
                };

                bool success = dbHelper.AddTrack(newTrack);
                if (success)
                {
                    MessageBox.Show("Песня успешно добавлена!");
                    if (PlaylistsComboBox.SelectedItem is Playlist pl)
                    {
                        LoadTracksForPlaylist(pl.PlaylistID);
                    }
                    else
                    {
                        LoadTracksForPlaylist(null);
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении песни.");
                }
            }
        }

        private void DeleteTrackButton_Click(object sender, RoutedEventArgs e)
        {
            if (TracksListView.SelectedItem is WfpMediaPlayer.Models.Track selectedTrack)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить трек '{selectedTrack.Title}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = dbHelper.RemoveTrack(selectedTrack.TrackID);
                        if (success)
                        {
                            MessageBox.Show("Трек успешно удален!");
                            if (MediaPlayer.Source != null && selectedTrack.FilePath == MediaPlayer.Source.LocalPath)
                            {
                                MediaPlayer.Stop();
                                MediaPlayer.Source = null;
                                ProgressSlider.Value = 0;
                                CurrentTimeDisplay.Text = "00:00";
                                TotalTimeDisplay.Text = "00:00";
                            }
                            if (PlaylistsComboBox.SelectedItem is Playlist pl)
                            {
                                LoadTracksForPlaylist(pl.PlaylistID);
                            }
                            else
                            {
                                LoadTracksForPlaylist(null);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить трек.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите трек для удаления!");
            }
        }

        private void AddArtistButton_Click(object sender, RoutedEventArgs e)
        {
            string artistName = PromptForInput("Введите имя артиста:");
            if (!string.IsNullOrEmpty(artistName))
            {
                Artist newArtist = new Artist { Name = artistName };
                if (dbHelper.AddArtist(newArtist))
                {
                    MessageBox.Show("Артист добавлен!");
                    LoadArtists();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении артиста.");
                }
            }
        }

        private void AddAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            string albumTitle = PromptForInput("Введите название альбома:");
            if (!string.IsNullOrEmpty(albumTitle))
            {
                Album newAlbum = new Album
                {
                    ArtistID = ArtistsComboBox.SelectedItem is Artist artist ? artist.ArtistID : 1,
                    Title = albumTitle
                };
                if (dbHelper.AddAlbum(newAlbum))
                {
                    MessageBox.Show("Альбом добавлен!");
                    LoadAlbums();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении альбома.");
                }
            }
        }

        private void AssignArtistButton_Click(object sender, RoutedEventArgs e)
        {
            if (TracksListView.SelectedItem is WfpMediaPlayer.Models.Track selectedTrack &&
                ArtistsComboBox.SelectedItem is Artist selectedArtist)
            {
                selectedTrack.ArtistID = selectedArtist.ArtistID;
                if (dbHelper.UpdateTrack(selectedTrack))
                {
                    MessageBox.Show("Артист назначен!");
                    if (PlaylistsComboBox.SelectedItem is Playlist pl)
                    {
                        LoadTracksForPlaylist(pl.PlaylistID);
                    }
                    else
                    {
                        LoadTracksForPlaylist(null);
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка при назначении артиста.");
                }
            }
            else
            {
                MessageBox.Show("Выберите трек и артиста!");
            }
        }

        private void AssignAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            if (TracksListView.SelectedItem is WfpMediaPlayer.Models.Track selectedTrack &&
                AlbumsComboBox.SelectedItem is Album selectedAlbum)
            {
                selectedTrack.AlbumID = selectedAlbum.AlbumID;
                if (dbHelper.UpdateTrack(selectedTrack))
                {
                    MessageBox.Show("Альбом назначен!");
                    if (PlaylistsComboBox.SelectedItem is Playlist pl)
                    {
                        LoadTracksForPlaylist(pl.PlaylistID);
                    }
                    else
                    {
                        LoadTracksForPlaylist(null);
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка при назначении альбома.");
                }
            }
            else
            {
                MessageBox.Show("Выберите трек и альбом!");
            }
        }

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string playlistName = PromptForInput("Введите название плейлиста:");
            if (!string.IsNullOrEmpty(playlistName))
            {
                Playlist newPlaylist = new Playlist { UserID = currentUser.UserID, Name = playlistName };
                if (dbHelper.AddPlaylist(newPlaylist))
                {
                    MessageBox.Show("Плейлист создан!");
                    LoadPlaylists();
                }
                else
                {
                    MessageBox.Show("Ошибка при создании плейлиста.");
                }
            }
        }

        private void AddToPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (TracksListView.SelectedItem is WfpMediaPlayer.Models.Track selectedTrack &&
                PlaylistsComboBox.SelectedItem is Playlist selectedPlaylist)
            {
                PlaylistTrack pt = new PlaylistTrack
                {
                    PlaylistID = selectedPlaylist.PlaylistID,
                    TrackID = selectedTrack.TrackID,
                    TrackOrder = 1
                };
                if (dbHelper.AddPlaylistTrack(pt))
                {
                    MessageBox.Show("Трек добавлен в плейлист!");
                    LoadTracksForPlaylist(selectedPlaylist.PlaylistID);
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении трека в плейлист.");
                }
            }
            else
            {
                MessageBox.Show("Выберите трек и плейлист!");
            }
        }

        private void PlaylistsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistsComboBox.SelectedItem is Playlist selectedPlaylist)
            {
                LoadTracksForPlaylist(selectedPlaylist.PlaylistID);
            }
            else
            {
                LoadTracksForPlaylist(null);
            }
        }

        private static string PromptForInput(string prompt)
        {
            InputDialog dialog = new InputDialog(prompt);
            if (dialog.ShowDialog() == true)
            {
                return dialog.Input;
            }
            return string.Empty;
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (trackDuration == TimeSpan.Zero && MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                trackDuration = MediaPlayer.NaturalDuration.TimeSpan;
                ProgressSlider.Maximum = trackDuration.TotalSeconds;
                TotalTimeDisplay.Text = trackDuration.ToString(@"mm\:ss");
            }
            ProgressSlider.Value = 0;
        }
    }
}