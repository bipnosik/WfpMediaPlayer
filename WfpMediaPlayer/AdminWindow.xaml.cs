using System.Windows;
using WfpMediaPlayer.Data;
using WfpMediaPlayer.Models;

namespace WfpMediaPlayer.Views
{
    public partial class AdminWindow : Window
    {
        private DatabaseHelper dbHelper;

        public AdminWindow()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadUsers();
        }

        private void LoadUsers()
        {
            UsersDataGrid.ItemsSource = dbHelper.GetUsers();
        }

        private void UsersDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Логика обработки выбора пользователя
        }


        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                EditUserWindow editWindow = new EditUserWindow(selectedUser);
                if (editWindow.ShowDialog() == true)
                {
                    bool success = dbHelper.UpdateUser(selectedUser);
                    if (success)
                    {
                        MessageBox.Show("Пользователь обновлен.");
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка обновления пользователя.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования.");
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                if (MessageBox.Show($"Вы уверены, что хотите удалить пользователя {selectedUser.Email}?",
                                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    bool success = dbHelper.DeleteUser(selectedUser.UserID);
                    if (success)
                    {
                        MessageBox.Show("Пользователь удален.");
                        LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка удаления пользователя.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для удаления.");
            }
        }
    }
}
