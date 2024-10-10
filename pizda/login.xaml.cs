using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace pizda
{
    /// <summary>
    /// Логика взаимодействия для login.xaml
    /// </summary>
    public partial class login : Window
    {
        string connectionString = "Server=localhost;Port=5432;Database=pizda;User Id=postgres;Password=123";
        private int loginAttempts = 0;
        private DispatcherTimer blockTimer;

        public login()
        {
            InitializeComponent();
            InitializeBlockTimer();
        }

        private void InitializeBlockTimer()
        {
            blockTimer = new DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(5);
            blockTimer.Tick += BlockTimer_Tick;
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            loginTextBox.IsEnabled = true;
            PasswordBox.IsEnabled = true;
            LoginButton.IsEnabled = true;
            blockTimer.Stop();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = loginTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM users WHERE login = @login AND password = @password";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", username); // Используем переменную username
                        command.Parameters.AddWithValue("@password", password);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                MessageBox.Show("Login successful!");
                                MainWindow mainWindow = new MainWindow();
                                mainWindow.Show();
                                this.Close();
                            }
                            else
                            {
                                loginAttempts++; // Увеличиваем счетчик попыток
                                MessageBox.Show("Invalid login or password.");

                                // Если достигнут лимит попыток
                                if (loginAttempts >= 3)
                                {
                                    // Блокируем элементы управления
                                    loginTextBox.IsEnabled = false;
                                    PasswordBox.IsEnabled = false;
                                    LoginButton.IsEnabled = false;

                                    // Запускаем таймер
                                    blockTimer.Start();
                                    MessageBox.Show("Too many failed attempts. Please wait for 5 seconds.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
