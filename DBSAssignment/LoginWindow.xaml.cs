using DBSAssignment;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Windows;
using System.Windows.Input;

namespace Login {
    public partial class LoginWindow : Window {
        public static string connectionString;
        public static OracleConnection connection;
        public static string username = "";

        public LoginWindow() {
            InitializeComponent();
        }
        private void Main_Window_Loaded(object _, RoutedEventArgs __) {
            userName.Focus();
        }

        private void userName_KeyDown(object _, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                password.Focus();
            }
        }
        private void password_KeyDown(object _, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Button_Click(null, null);
            }
        }
        private void Button_Click(object _, RoutedEventArgs __) {
            try {
                username = userName.Text;
                connectionString = $"User Id={username};Password={password.Password};Data Source=localhost:1521/orcl";
                connection = new OracleConnection(connectionString);
                connection.Open();
                new MainWindow().Show();
                this.Close();
            } catch(Exception ex) { 
                MessageBox.Show("Error" + ex.Message);
                userName.Clear();
                password.Clear();
                userName.Focus();
            }
        }
    }
}