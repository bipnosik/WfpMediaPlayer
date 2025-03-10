using System.Windows;
using System.Windows.Controls;
using WfpMediaPlayer.Models;

namespace WfpMediaPlayer.Views
{
    public partial class EditUserWindow : Window
    {
        public User User { get; set; }

        public EditUserWindow(User user)
        {
            InitializeComponent();
            User = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            EmailTextBox.Text = User.Email;
            // Изначально выводим пароль в маскированном виде
            PasswordBox.Password = User.PasswordHash;
            FirstNameTextBox.Text = User.FirstName;
            LastNameTextBox.Text = User.LastName;
            RoleTextBox.Text = User.Role;
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // При включенном чекбоксе показываем пароль в открытом виде
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // При выключенном чекбоксе возвращаем маскированное отображение
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            User.Email = EmailTextBox.Text;
            // Если отображается маскированное поле, берем значение из PasswordBox; иначе из PasswordTextBox
            if (PasswordBox.Visibility == Visibility.Visible)
                User.PasswordHash = PasswordBox.Password;
            else
                User.PasswordHash = PasswordTextBox.Text;

            User.FirstName = FirstNameTextBox.Text;
            User.LastName = LastNameTextBox.Text;
            User.Role = RoleTextBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}
