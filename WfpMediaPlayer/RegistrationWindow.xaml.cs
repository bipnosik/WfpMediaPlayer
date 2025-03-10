using System.Windows;
using WfpMediaPlayer.Data;
using WfpMediaPlayer.Models;

namespace WfpMediaPlayer.Views
{
    public partial class RegistrationWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;

            // Простейшая проверка обязательных полей
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Заполните обязательные поля (Email и Пароль).");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.");
                return;
            }

            // Создаем нового пользователя
            User newUser = new User
            {
                Email = email,
                PasswordHash = password,  // Для демонстрации пароль сохраняется в открытом виде. В реальном проекте используйте хеширование.
                FirstName = firstName,
                LastName = lastName,
                Role = "User"  // По умолчанию регистрируются обычные пользователи
            };

            bool success = dbHelper.AddUser(newUser);
            if (success)
            {
                MessageBox.Show("Регистрация прошла успешно!");
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка регистрации.");
            }
        }
    }
}
