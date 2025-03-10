using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using WfpMediaPlayer.Models;

namespace WfpMediaPlayer.Data
{
    public class DatabaseHelper
    {
        private string connectionString = @"Server=DESKTOP-DSVMJPM\SQLEXSPRESS;Database=CapybaraMusicDB;Trusted_Connection=True;";

        public List<WfpMediaPlayer.Models.Track> GetTracks()
        {
            List<WfpMediaPlayer.Models.Track> tracks = new List<WfpMediaPlayer.Models.Track>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT t.TrackID, t.Title, t.ArtistID, t.AlbumID, t.Duration, t.Genre, t.FilePath, t.Format,
                           a.Name AS ArtistName, al.Title AS AlbumTitle
                    FROM Tracks t
                    LEFT JOIN Artists a ON t.ArtistID = a.ArtistID
                    LEFT JOIN Albums al ON t.AlbumID = al.AlbumID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tracks.Add(new WfpMediaPlayer.Models.Track
                            {
                                TrackID = (int)reader["TrackID"],
                                Title = reader["Title"].ToString(),
                                ArtistID = (int)reader["ArtistID"],
                                AlbumID = reader["AlbumID"] != DBNull.Value ? (int?)reader["AlbumID"] : null,
                                Duration = reader["Duration"] != DBNull.Value ? (TimeSpan?)TimeSpan.Parse(reader["Duration"].ToString()) : null,
                                Genre = reader["Genre"].ToString(),
                                FilePath = reader["FilePath"].ToString(),
                                Format = reader["Format"].ToString(),
                                ArtistName = reader["ArtistName"].ToString(),
                                AlbumTitle = reader["AlbumTitle"] != DBNull.Value ? reader["AlbumTitle"].ToString() : null
                            });
                        }
                    }
                }
            }
            return tracks;
        }
        public User AuthenticateUser(string email, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT UserID, Email, PasswordHash, FirstName, LastName, RegistrationDate, Role 
                                 FROM Users 
                                 WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPassword = reader["PasswordHash"].ToString();
                            // Простой пример сравнения (в реальном проекте используйте хеширование паролей)
                            if (password == storedPassword)
                            {
                                return new User
                                {
                                    UserID = (int)reader["UserID"],
                                    Email = reader["Email"].ToString(),
                                    PasswordHash = storedPassword,
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    RegistrationDate = (DateTime)reader["RegistrationDate"],
                                    Role = reader["Role"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }
        public bool RemoveTrack(int trackId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Удаляем связанные записи из PlaylistTracks
                    string deletePlaylistTracksQuery = "DELETE FROM PlaylistTracks WHERE TrackID = @TrackID";
                    using (SqlCommand cmd = new SqlCommand(deletePlaylistTracksQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TrackID", trackId);
                        cmd.ExecuteNonQuery();
                    }

                    // Удаляем трек
                    string deleteTrackQuery = "DELETE FROM Tracks WHERE TrackID = @TrackID";
                    using (SqlCommand cmd = new SqlCommand(deleteTrackQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TrackID", trackId);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления трека: " + ex.Message);
                return false;
            }
        }
        public bool UpdateTrack(WfpMediaPlayer.Models.Track track)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        UPDATE Tracks 
                        SET Title = @Title, ArtistID = @ArtistID, AlbumID = @AlbumID, Duration = @Duration, 
                            Genre = @Genre, FilePath = @FilePath, Format = @Format
                        WHERE TrackID = @TrackID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TrackID", track.TrackID);
                        cmd.Parameters.AddWithValue("@Title", track.Title);
                        cmd.Parameters.AddWithValue("@ArtistID", track.ArtistID);
                        cmd.Parameters.AddWithValue("@AlbumID", track.AlbumID.HasValue ? (object)track.AlbumID.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Duration", track.Duration.HasValue ? (object)track.Duration.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Genre", track.Genre);
                        cmd.Parameters.AddWithValue("@FilePath", track.FilePath);
                        cmd.Parameters.AddWithValue("@Format", track.Format);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка обновления трека: " + ex.Message);
                return false;
            }
        }
        public List<User> GetUsers()
        {
            List<User> users = new List<User>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT UserID, Email, PasswordHash, FirstName, LastName, Role FROM Users";
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            UserID = reader.GetInt32(0),
                            Email = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            FirstName = reader.GetString(3),
                            LastName = reader.GetString(4),
                            Role = reader.GetString(5)
                        });
                    }
                }
            }
            return users;
        }
        public bool AddUser(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Users (Email, PasswordHash, FirstName, LastName, RegistrationDate, Role) " +
                                   "VALUES (@Email, @PasswordHash, @FirstName, @LastName, GETDATE(), @Role)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                        command.Parameters.AddWithValue("@FirstName", user.FirstName);
                        command.Parameters.AddWithValue("@LastName", user.LastName);
                        command.Parameters.AddWithValue("@Role", user.Role);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message);
                return false;
            }
        }
        public bool AddTrack(WfpMediaPlayer.Models.Track track)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO Tracks (Title, ArtistID, AlbumID, Duration, Genre, FilePath, Format)
                        VALUES (@Title, @ArtistID, @AlbumID, @Duration, @Genre, @FilePath, @Format)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title", track.Title);
                        cmd.Parameters.AddWithValue("@ArtistID", track.ArtistID);
                        cmd.Parameters.AddWithValue("@AlbumID", track.AlbumID.HasValue ? (object)track.AlbumID.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Duration", track.Duration.HasValue ? (object)track.Duration.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Genre", track.Genre);
                        cmd.Parameters.AddWithValue("@FilePath", track.FilePath);
                        cmd.Parameters.AddWithValue("@Format", track.Format);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления трека: " + ex.Message);
                return false;
            }
        }




        #region Artists

        public bool AddArtist(Artist artist)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Artists (Name, Biography, Country) VALUES (@Name, @Biography, @Country)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", artist.Name);
                        cmd.Parameters.AddWithValue("@Biography", string.IsNullOrEmpty(artist.Biography) ? (object)DBNull.Value : artist.Biography);
                        cmd.Parameters.AddWithValue("@Country", string.IsNullOrEmpty(artist.Country) ? (object)DBNull.Value : artist.Country);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка добавления артиста: " + ex.Message);
                return false;
            }
        }

        public bool UpdateArtist(Artist artist)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Artists SET Name = @Name, Biography = @Biography, Country = @Country WHERE ArtistID = @ArtistID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", artist.Name);
                        cmd.Parameters.AddWithValue("@Biography", string.IsNullOrEmpty(artist.Biography) ? (object)DBNull.Value : artist.Biography);
                        cmd.Parameters.AddWithValue("@Country", string.IsNullOrEmpty(artist.Country) ? (object)DBNull.Value : artist.Country);
                        cmd.Parameters.AddWithValue("@ArtistID", artist.ArtistID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка обновления артиста: " + ex.Message);
                return false;
            }
        }

        public bool DeleteArtist(int artistID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Artists WHERE ArtistID = @ArtistID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ArtistID", artistID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка удаления артиста: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Albums

        public bool AddAlbum(Album album)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Albums (ArtistID, Title, ReleaseDate, CoverImagePath) VALUES (@ArtistID, @Title, @ReleaseDate, @CoverImagePath)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ArtistID", album.ArtistID);
                        cmd.Parameters.AddWithValue("@Title", album.Title);
                        cmd.Parameters.AddWithValue("@ReleaseDate", album.ReleaseDate.HasValue ? (object)album.ReleaseDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@CoverImagePath", string.IsNullOrEmpty(album.CoverImagePath) ? (object)DBNull.Value : album.CoverImagePath);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка добавления альбома: " + ex.Message);
                return false;
            }
        }

        public bool UpdateAlbum(Album album)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Albums SET ArtistID = @ArtistID, Title = @Title, ReleaseDate = @ReleaseDate, CoverImagePath = @CoverImagePath WHERE AlbumID = @AlbumID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ArtistID", album.ArtistID);
                        cmd.Parameters.AddWithValue("@Title", album.Title);
                        cmd.Parameters.AddWithValue("@ReleaseDate", album.ReleaseDate.HasValue ? (object)album.ReleaseDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@CoverImagePath", string.IsNullOrEmpty(album.CoverImagePath) ? (object)DBNull.Value : album.CoverImagePath);
                        cmd.Parameters.AddWithValue("@AlbumID", album.AlbumID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка обновления альбома: " + ex.Message);
                return false;
            }
        }

        public bool DeleteAlbum(int albumID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Albums WHERE AlbumID = @AlbumID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AlbumID", albumID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка удаления альбома: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Playlists

        public bool AddPlaylist(Playlist playlist)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Playlists (UserID, Name, Description) VALUES (@UserID, @Name, @Description)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", playlist.UserID);
                        cmd.Parameters.AddWithValue("@Name", playlist.Name);
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(playlist.Description) ? (object)DBNull.Value : playlist.Description);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка добавления плейлиста: " + ex.Message);
                return false;
            }
        }

        public bool UpdatePlaylist(Playlist playlist)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Playlists SET UserID = @UserID, Name = @Name, Description = @Description WHERE PlaylistID = @PlaylistID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", playlist.UserID);
                        cmd.Parameters.AddWithValue("@Name", playlist.Name);
                        cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(playlist.Description) ? (object)DBNull.Value : playlist.Description);
                        cmd.Parameters.AddWithValue("@PlaylistID", playlist.PlaylistID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка обновления плейлиста: " + ex.Message);
                return false;
            }
        }

        public bool DeletePlaylist(int playlistID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Playlists WHERE PlaylistID = @PlaylistID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PlaylistID", playlistID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка удаления плейлиста: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region PlaylistTracks

        public bool AddPlaylistTrack(PlaylistTrack pt)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO PlaylistTracks (PlaylistID, TrackID, TrackOrder) VALUES (@PlaylistID, @TrackID, @TrackOrder)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PlaylistID", pt.PlaylistID);
                        cmd.Parameters.AddWithValue("@TrackID", pt.TrackID);
                        cmd.Parameters.AddWithValue("@TrackOrder", pt.TrackOrder);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка добавления трека в плейлист: " + ex.Message);
                return false;
            }
        }

        public bool DeletePlaylistTrack(int playlistID, int trackID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM PlaylistTracks WHERE PlaylistID = @PlaylistID AND TrackID = @TrackID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PlaylistID", playlistID);
                        cmd.Parameters.AddWithValue("@TrackID", trackID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка удаления трека из плейлиста: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Subscriptions

        public bool AddSubscription(Subscription subscription)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Subscriptions (UserID, SubscriptionType, StartDate, EndDate) VALUES (@UserID, @SubscriptionType, @StartDate, @EndDate)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", subscription.UserID);
                        cmd.Parameters.AddWithValue("@SubscriptionType", subscription.SubscriptionType);
                        cmd.Parameters.AddWithValue("@StartDate", subscription.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", subscription.EndDate.HasValue ? (object)subscription.EndDate.Value : DBNull.Value);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка добавления подписки: " + ex.Message);
                return false;
            }
        }

        public bool UpdateSubscription(Subscription subscription)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Subscriptions SET UserID = @UserID, SubscriptionType = @SubscriptionType, StartDate = @StartDate, EndDate = @EndDate WHERE SubscriptionID = @SubscriptionID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", subscription.UserID);
                        cmd.Parameters.AddWithValue("@SubscriptionType", subscription.SubscriptionType);
                        cmd.Parameters.AddWithValue("@StartDate", subscription.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", subscription.EndDate.HasValue ? (object)subscription.EndDate.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@SubscriptionID", subscription.SubscriptionID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка обновления подписки: " + ex.Message);
                return false;
            }
        }
        public bool UpdateUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Users SET Email = @Email, PasswordHash = @Password, FirstName = @FirstName, LastName = @LastName, Role = @Role WHERE UserID = @UserID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@Password", user.PasswordHash); // Хранение пароля в открытом виде не рекомендуется
                        command.Parameters.AddWithValue("@FirstName", user.FirstName);
                        command.Parameters.AddWithValue("@LastName", user.LastName);
                        command.Parameters.AddWithValue("@Role", user.Role);
                        command.Parameters.AddWithValue("@UserID", user.UserID);

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при обновлении пользователя: " + ex.Message);
                    return false;
                }
            }
        }
        public List<WfpMediaPlayer.Models.Track> GetTracksByPlaylist(int playlistId)
        {
            List<WfpMediaPlayer.Models.Track> tracks = new List<WfpMediaPlayer.Models.Track>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT t.TrackID, t.Title, t.ArtistID, t.AlbumID, t.Duration, t.Genre, t.FilePath, t.Format,
                           a.Name AS ArtistName, al.Title AS AlbumTitle
                    FROM Tracks t
                    INNER JOIN PlaylistTracks pt ON t.TrackID = pt.TrackID
                    LEFT JOIN Artists a ON t.ArtistID = a.ArtistID
                    LEFT JOIN Albums al ON t.AlbumID = al.AlbumID
                    WHERE pt.PlaylistID = @PlaylistID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PlaylistID", playlistId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tracks.Add(new WfpMediaPlayer.Models.Track
                            {
                                TrackID = (int)reader["TrackID"],
                                Title = reader["Title"].ToString(),
                                ArtistID = (int)reader["ArtistID"],
                                AlbumID = reader["AlbumID"] != DBNull.Value ? (int?)reader["AlbumID"] : null,
                                Duration = reader["Duration"] != DBNull.Value ? (TimeSpan?)TimeSpan.Parse(reader["Duration"].ToString()) : null,
                                Genre = reader["Genre"].ToString(),
                                FilePath = reader["FilePath"].ToString(),
                                Format = reader["Format"].ToString(),
                                ArtistName = reader["ArtistName"].ToString(),
                                AlbumTitle = reader["AlbumTitle"] != DBNull.Value ? reader["AlbumTitle"].ToString() : null
                            });
                        }
                    }
                }
            }
            return tracks;
        }


        public bool DeleteUser(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Users WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка удаления пользователя: " + ex.Message);
                return false;
            }
        }

        #endregion


        public bool DeleteSubscription(int subscriptionID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM Subscriptions WHERE SubscriptionID = @SubscriptionID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SubscriptionID", subscriptionID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка удаления подписки: " + ex.Message);
                return false;
            }
        }



        #region PaymentTransactions

        public bool AddPaymentTransaction(PaymentTransaction transaction)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO PaymentTransactions (UserID, PaymentDate, PaymentAmount, PaymentMethod, PaymentStatus) VALUES (@UserID, @PaymentDate, @PaymentAmount, @PaymentMethod, @PaymentStatus)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", transaction.UserID);
                        cmd.Parameters.AddWithValue("@PaymentDate", transaction.PaymentDate);
                        cmd.Parameters.AddWithValue("@PaymentAmount", transaction.PaymentAmount);
                        cmd.Parameters.AddWithValue("@PaymentMethod", transaction.PaymentMethod);
                        cmd.Parameters.AddWithValue("@PaymentStatus", transaction.PaymentStatus);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка добавления платежной транзакции: " + ex.Message);
                return false;
            }
        }

        public bool UpdatePaymentTransaction(PaymentTransaction transaction)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE PaymentTransactions SET UserID = @UserID, PaymentDate = @PaymentDate, PaymentAmount = @PaymentAmount, PaymentMethod = @PaymentMethod, PaymentStatus = @PaymentStatus WHERE PaymentID = @PaymentID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", transaction.UserID);
                        cmd.Parameters.AddWithValue("@PaymentDate", transaction.PaymentDate);
                        cmd.Parameters.AddWithValue("@PaymentAmount", transaction.PaymentAmount);
                        cmd.Parameters.AddWithValue("@PaymentMethod", transaction.PaymentMethod);
                        cmd.Parameters.AddWithValue("@PaymentStatus", transaction.PaymentStatus);
                        cmd.Parameters.AddWithValue("@PaymentID", transaction.PaymentID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка обновления платежной транзакции: " + ex.Message);
                return false;
            }
        }
        public List<Playlist> GetPlaylists(int userId)
        {
            List<Playlist> playlists = new List<Playlist>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT PlaylistID, UserID, Name, Description, CreationDate FROM Playlists WHERE UserID = @UserID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            playlists.Add(new Playlist
                            {
                                PlaylistID = (int)reader["PlaylistID"],
                                UserID = (int)reader["UserID"],
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                CreationDate = (DateTime)reader["CreationDate"]
                            });
                        }
                    }
                }
            }
            return playlists;
        }

        public bool DeletePaymentTransaction(int paymentID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM PaymentTransactions WHERE PaymentID = @PaymentID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PaymentID", paymentID);
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка удаления платежной транзакции: " + ex.Message);
                return false;
            }
        }
        public List<Artist> GetArtists()
        {
            List<Artist> artists = new List<Artist>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT ArtistID, Name FROM Artists";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            artists.Add(new Artist
                            {
                                ArtistID = (int)reader["ArtistID"],
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return artists;
        }
        public List<Album> GetAlbums()
        {
            List<Album> albums = new List<Album>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT AlbumID, ArtistID, Title FROM Albums";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            albums.Add(new Album
                            {
                                AlbumID = (int)reader["AlbumID"],
                                ArtistID = (int)reader["ArtistID"],
                                Title = reader["Title"].ToString()
                            });
                        }
                    }
                }
            }
            return albums;
        }

        #endregion
    }
}