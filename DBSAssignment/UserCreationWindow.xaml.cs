using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1;

namespace DBSAssignment
{
    public partial class UserCreationWindow : Window
    {

        public UserCreationWindow()
        {
            InitializeComponent();
           
        }

        private void btnCreateUser_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            //Creating an user
            string sql = $"CREATE USER {username} IDENTIFIED BY {password}";
            ExecuteNonQuery(sql);

            //Granting privileges for the new user
            if (chkSession.IsChecked == true)
            {
                sql = $"GRANT create session TO {username}";
                ExecuteNonQuery(sql);
            }

            if (chkSelect.IsChecked == true)
            {
                foreach (string table in Window1.userTableNames)
                {
                    sql = $"GRANT select ON {table} TO {username}";
                    ExecuteNonQuery(sql);
                } 
            }

            if (chkInsert.IsChecked == true)
            {
                foreach (string table in Window1.userTableNames)
                {
                    sql = $"GRANT insert ON {table} TO {username}";
                    ExecuteNonQuery(sql);
                }
            }

            if (chkUpdate.IsChecked == true)
            {
                foreach (string table in Window1.userTableNames)
                {
                    sql = $"GRANT update ON {table} TO {username}";
                    ExecuteNonQuery(sql);
                }
            }

            if (chkDelete.IsChecked == true)
            {
                foreach (string table in Window1.userTableNames)
                {
                    sql = $"GRANT delete ON {table} TO {username}";
                    ExecuteNonQuery(sql);
                }
            }

            MessageBox.Show("User created successfully with selected privileges.");
        }

        private void ExecuteNonQuery(string sql)
        {
            OracleCommand command = new OracleCommand();
            command.Connection = MainWindow.connection;
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            command.ExecuteNonQuery();
        }

        private void chkSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            // Check all checkboxes when "Select All" is checked
            chkSession.IsChecked = chkSelect.IsChecked = chkInsert.IsChecked = chkUpdate.IsChecked = chkDelete.IsChecked = true;
        }

        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            // Uncheck all checkboxes when "Select All" is unchecked
            chkSession.IsChecked = chkSelect.IsChecked = chkInsert.IsChecked = chkUpdate.IsChecked = chkDelete.IsChecked = false;
        }

        private void btn_Home_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}
