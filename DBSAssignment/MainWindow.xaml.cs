using DBSAssignment;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1 {
    public partial class MainWindow : Window {
        public static string connectionString;
        public static OracleConnection connection;
        public static string username = "";

        public MainWindow() {
            InitializeComponent();
        }
        private void Main_Window_Loaded(object sender, RoutedEventArgs e) {
            userName.Focus();
        }

        private void userName_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                password.Focus();
            }
        }
        private void password_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Button_Click(null, null);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e) {
            try {
                username = userName.Text;
                connectionString = $"User Id={username};Password={password.Password};Data Source=localhost:1521/orcl";
                connection = new OracleConnection(connectionString);
                connection.Open();
                new Window1().Show();
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