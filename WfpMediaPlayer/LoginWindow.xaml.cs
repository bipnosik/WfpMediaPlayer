using System.Windows;
using WfpMediaPlayer.Data;
using WfpMediaPlayer.Models;

namespace WfpMediaPlayer.Views
{
    public partial class LoginWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordTextBox.Password;

            User user = dbHelper.AuthenticateUser(email, password);
            if (user != null)
            {
                if (user.Role == "Admin")
                {
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.Show();
                }
                else
                {
                    MainWindow mainWindow = new MainWindow(user);
                    mainWindow.Show();
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный email или пароль.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow regWindow = new RegistrationWindow();
            regWindow.ShowDialog();
        }
    }
}
