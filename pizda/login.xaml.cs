using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace pizda
{
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
                MessageBox.Show("Ладно.");
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
                        command.Parameters.AddWithValue("@login", username);
                        command.Parameters.AddWithValue("@password", password);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                MessageBox.Show("Ты вошёл!");
                                MainWindow mainWindow = new MainWindow();
                                mainWindow.Show();
                                this.Close();
                            }
                            else
                            {
                                loginAttempts++; 
                                MessageBox.Show("иди лесом.");

                                if (loginAttempts >= 3)
                                {
                                    loginTextBox.IsEnabled = false;
                                    PasswordBox.IsEnabled = false;
                                    LoginButton.IsEnabled = false;

                                    blockTimer.Start();
                                    MessageBox.Show("Жди плебей.");
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
