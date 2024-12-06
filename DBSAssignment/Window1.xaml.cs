using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using WpfApp1;
using System.Reflection;
using System.Drawing;
using System.Windows.Input;

namespace DBSAssignment
{
    public partial class Window1 : Window
    {
        OracleDataAdapter adapter;
        DataTable dataTable;
        Label label;
        TextBox textBox;
        List<FrameworkElement> textBoxesList = new List<FrameworkElement>();
        public static List<string> userTableNames;

        public Window1()
        {
            InitializeComponent();
            userTableNames = GetUserTableNames(MainWindow.connectionString);
            comboTables.ItemsSource = userTableNames;

            comboTables.SelectedIndex = 0;

            // Load data for the default selection
            if (comboTables.SelectedItem != null)
            {
                LoadData(comboTables.SelectedItem.ToString());
            }
        }

        static List<string> GetUserTableNames(string connectionString)
        {
            List<string> userTableNames = new List<string>();

            try
            {
                string query = "SELECT TABLE_NAME FROM USER_TABLES";

                using (OracleCommand command = new OracleCommand(query, MainWindow.connection))
                {
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Get the table name from the result set
                            string tableName = reader["TABLE_NAME"].ToString();
                            userTableNames.Add(tableName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return userTableNames;
        }

        private void LoadData(string tableName)
        {
            string sqlQuery = $"SELECT * FROM {tableName}";

            try
            {
                using (OracleCommand command = new OracleCommand(sqlQuery, MainWindow.connection))
                {
                    using (adapter = new OracleDataAdapter(command))
                    {
                        dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataTable.TableName = tableName;

                        // Bind the DataTable to a DataGrid or perform other actions with the data
                        dataGrid.ItemsSource = dataTable.DefaultView;

                        CreateLabelsAndControls(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get the selected item content from the ComboBox
            if (comboTables.SelectedItem != null)
            {
                string selectedTable = comboTables.SelectedItem.ToString();

                // Load data based on the selected table
                LoadData(selectedTable);
            }
        }

        private void CreateLabelsAndControls(DataTable dataTable)
        {
            // Clear existing content in the container
            columnsInformation.Children.Clear();
            columnsInformation.ColumnDefinitions.Clear();
            columnsInformation.RowDefinitions.Clear();

            textBoxesList = new List<FrameworkElement>();

            int row = 0;

            foreach (DataColumn column in dataTable.Columns)
            {
                // Create a label
                label = new Label
                {
                    Name = $"{column.ColumnName}Label",
                    Content = column.ColumnName,
                    Margin = new Thickness(5),
                    Foreground = System.Windows.Media.Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                // Check the data type of the column
                TypeCode typeCode = Type.GetTypeCode(column.DataType);

                if (typeCode == TypeCode.DateTime)
                {
                    // Create a DatePicker for date columns
                    DatePicker datePicker = new DatePicker
                    {
                        Name = $"{column.ColumnName}DatePicker", // You can set a unique name for each DatePicker if needed
                        Width = 160,
                        Margin = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };

                    textBoxesList.Add(datePicker);

                    Grid.SetRow(label, row);
                    Grid.SetColumn(label, 0);
                    Grid.SetRow(datePicker, row);
                    Grid.SetColumn(datePicker, 1);
                    columnsInformation.Children.Add(label);
                    columnsInformation.Children.Add(datePicker);
                }
                else
                {
                    // Create a TextBox for string columns
                    textBox = new TextBox
                    {
                        Name = $"{column.ColumnName}TextBox", // You can set a unique name for each TextBox if needed
                        Width = 160,
                        Margin = new Thickness(5),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    textBoxesList.Add(textBox);

                    Grid.SetRow(label, row);
                    Grid.SetColumn(label, 0);
                    Grid.SetRow(textBox, row);
                    Grid.SetColumn(textBox, 1);
                    columnsInformation.Children.Add(label);
                    columnsInformation.Children.Add(textBox);
                }
                columnsInformation.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                row++;
            }
        }

        private void Insert_btn_Click(object sender, RoutedEventArgs e)
        {
            if (comboTables.SelectedItem != null)
            {
                string sql = $"INSERT INTO {dataTable.TableName}(";
                string tempSql = $"INSERT INTO {dataTable.TableName}(";

                foreach (DataColumn column in dataTable.Columns)
                {
                    sql += $"{column.ColumnName},";
                    tempSql += $"{column.ColumnName},";
                }

                sql = sql.Substring(0, sql.Length - 1) + ")" + "\nVALUES(";
                tempSql = tempSql.Substring(0, tempSql.Length - 1) + ")" + "\nVALUES(";

                int counter = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.ColumnName != null)
                    {
                        if (columnsInformation.Children[counter] is TextBox)
                        {
                            sql += $"'{(columnsInformation.Children[counter] as TextBox).Text}', ";
                            tempSql += "'', ";
                        }
                        else if (columnsInformation.Children[counter] is DatePicker)
                        {
                            sql += $"TO_DATE('{(columnsInformation.Children[counter] as DatePicker).SelectedDate}', 'DD-Mon-YY HH:MI:SS AM'), ";
                            tempSql += $"TO_DATE('', 'DD-Mon-YY HH:MI:SS AM'), ";
                        }
                    }
                    counter += 2;
                }

                sql = sql.Substring(0, sql.Length - 2) + ")";
                tempSql = tempSql.Substring(0, tempSql.Length - 2) + ")";

                if (sql != tempSql)
                {
                    IUD(sql, 0);

                    Insert_btn.IsEnabled = false;
                    Update_btn.IsEnabled = true;
                    Delete_btn.IsEnabled = true;
                }
                else
                    MessageBox.Show("You can not insert empty row!");
            }

        }

        private void Update_btn_Click(object sender, RoutedEventArgs e)
        {
            if (comboTables.SelectedItem != null)
            {
                string sql = $"UPDATE {dataTable.TableName} SET ";
                int counter = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.ColumnName != null)
                    {
                        if(columnsInformation.Children[counter] is TextBox)
                            sql += $"{column.ColumnName} = '{(columnsInformation.Children[counter] as TextBox).Text}',";
                        else if(columnsInformation.Children[counter] is DatePicker)
                            sql += $"{column.ColumnName} = TO_DATE('{(columnsInformation.Children[counter] as DatePicker).SelectedDate}', 'DD-Mon-YY HH:MI:SS AM'),";

                    }
                    counter += 2;
                }

                DataRowView selectedRow = dataGrid.SelectedItem as DataRowView;

                if (selectedRow != null)
                {
                    object selectedValue = selectedRow[dataTable.Columns[0].ColumnName];

                    sql = sql.Substring(0, sql.Length - 1) + $"\nWHERE {dataTable.Columns[0].ColumnName} = {selectedValue}";
                    IUD(sql, 1);
                }
            }
        }

        private void Delete_btn_Click(object sender, RoutedEventArgs e)
        {
            string sql = "";
            if (comboTables.SelectedItem != null)
            { 
                sql = $"DELETE FROM {dataTable.TableName} WHERE {dataTable.Columns[0].ColumnName} = '{(columnsInformation.Children[1] as TextBox).Text}'";

                IUD(sql, 2);
            }
            resetAll();
        }

        private void resetAll()
        {
            int counter = 0;
            if(comboTables.SelectedItem != null)
                foreach(DataColumn column in dataTable.Columns)
                {
                    TypeCode typeCode = Type.GetTypeCode(column.DataType);

                    if(typeCode == TypeCode.DateTime)
                    {
                        if (textBoxesList.ToArray()[counter] is DatePicker)
                        {
                            DatePicker datePicker = (DatePicker)textBoxesList.ToArray()[counter];
                            datePicker.SelectedDate = null;
                        }
                    }
                    else
                    {
                        TextBox textBox = (TextBox)textBoxesList.ToArray()[counter];
                        textBox.Text = "";
                    }
                    counter++;
                }

            Insert_btn.IsEnabled = true;
            Update_btn.IsEnabled = false;
            Delete_btn.IsEnabled = false;
        }

        private void Reset_btn_Click(object sender, RoutedEventArgs e)
        {
            resetAll();
        }

        private void IUD(string sqlStmt, int op)
        {
            string msg = "";
            OracleCommand command = new OracleCommand();
            command.Connection = MainWindow.connection;
            command.CommandText = sqlStmt;
            command.CommandType = CommandType.Text;

            try
            {
                switch (op)
                {
                    case 0:
                        msg = "Row inserted successfully!";
                        int counter = 0;

                        foreach (DataColumn column in dataTable.Columns)
                        {
                            TypeCode columnType = Type.GetTypeCode(column.DataType);

                            if (columnType == TypeCode.DateTime)
                            {
                                DatePicker datePicker = (DatePicker)textBoxesList.ToArray()[counter];
                                command.Parameters.Add(column.ColumnName, OracleDbType.Date, 7).Value = datePicker.SelectedDate;
                            }
                            else
                            {
                                TextBox textBox = (TextBox)textBoxesList.ToArray()[counter];
                                command.Parameters.Add(column.ColumnName, OracleDbType.Varchar2, 25).Value = textBox.Text;
                            }
                            counter++;
                        }
                        break;
                    case 1:
                        msg = "Row updated successfully!";
                        counter = 0;

                        foreach (DataColumn column in dataTable.Columns)
                        {
                            TypeCode columnType = Type.GetTypeCode(column.DataType);

                            if (columnType == TypeCode.DateTime)
                            {
                                DatePicker datePicker = (DatePicker)textBoxesList.ToArray()[counter];
                                command.Parameters.Add(column.ColumnName, OracleDbType.Date, 7).Value = datePicker.SelectedDate;
                            }
                            else
                            {
                                TextBox textBox = (TextBox)textBoxesList.ToArray()[counter];
                                command.Parameters.Add(column.ColumnName, OracleDbType.Varchar2, 25).Value = textBox.Text;
                            }
                            counter++;
                        }
                        break;

                    case 2:
                        msg = "Row deleted successfully!";
                        counter = 0;
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            TypeCode typeCode = Type.GetTypeCode(column.DataType);

                            if (typeCode == TypeCode.DateTime)
                            {
                                if (textBoxesList.ToArray()[counter] is DatePicker)
                                {
                                    DatePicker datePicker = (DatePicker)textBoxesList.ToArray()[counter];
                                    command.Parameters.Add(column.ColumnName, OracleDbType.Date, 7).Value = datePicker.SelectedDate;
                                }
                            }
                            else
                            {
                                TextBox textBox = (TextBox)textBoxesList.ToArray()[counter];
                                command.Parameters.Add(column.ColumnName, OracleDbType.Varchar2, 25).Value = textBox.Text;
                            }
                            counter++;
                        }
                        break;
                }

                int n = command.ExecuteNonQuery();
                if (n > 0)
                {
                    MessageBox.Show(msg);
                    LoadData(comboTables.SelectedItem.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                command.Dispose();
            }
        }
 

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem is DataRowView)
            {
                DataGrid dg = (DataGrid)sender;
                DataRowView dr = (DataRowView)dg.SelectedItem;

                int counter = 0;

                if (dr != null)
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        TypeCode typeCode = Type.GetTypeCode(column.DataType);

                        if (typeCode == TypeCode.DateTime)
                        {
                            DatePicker datePicker = (DatePicker)textBoxesList.ToArray()[counter];
                            if (dr[column.ColumnName] != DBNull.Value)
                                datePicker.SelectedDate = DateTime.Parse(dr[column.ColumnName].ToString());
                            else
                                datePicker.SelectedDate = null;
                        }
                        else
                        {
                            TextBox textBox = (TextBox)textBoxesList.ToArray()[counter];
                            textBox.Text = dr[column.ColumnName].ToString();
                        }

                        counter++;
                    }
                }
            }

            Insert_btn.IsEnabled = false;
            Update_btn.IsEnabled = true;
            Delete_btn.IsEnabled = true;
        }

        
        static void GeneratePdfReport(DataGrid dataGrid, string outputFilePath)
        {
            // Create a new MigraDoc document
            Document document = new Document();

            // Assuming A4 page size
            double totalA4Width = document.DefaultPageSetup.PageWidth.Centimeter;

            // Define a maximum width for each page (you can adjust this value)
            double maxPageWidth = totalA4Width;

            // Calculate the number of columns per page based on the maximum page width
            int columnsPerPage = (int)Math.Floor(maxPageWidth); // Assuming DefaultColumnWidth is a constant or calculated value

            // Add headers and define columns
            int columnCount = dataGrid.Columns.Count;

            for (int pageIndex = 0; pageIndex * columnsPerPage < columnCount; pageIndex++)
            {
                // Add a new section for each page
                Section section = document.AddSection();
                section.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape;

                // Add a table to the section
                MigraDoc.DocumentObjectModel.Tables.Table table = section.AddTable();
                table.Borders.Width = 0.75;

                // Add headers and define columns for the current page
                int startIndex = pageIndex * columnsPerPage;
                int endIndex = Math.Min(startIndex + columnsPerPage, columnCount);

                for (int i = startIndex; i < endIndex; i++)
                {
                    string headerText = dataGrid.Columns[i].Header.ToString();
                    double columnWidth = CalculateColumnWidth(headerText);

                    // Ensure that the column width is less than or equal to the total A4 width
                    if (columnWidth > totalA4Width)
                    {
                        columnWidth = totalA4Width;
                    }

                    table.AddColumn(Unit.FromCentimeter(columnWidth));
                }

                MigraDoc.DocumentObjectModel.Tables.Row headerRow = table.AddRow();
                for (int i = startIndex; i < endIndex; i++)
                {
                    headerRow.Cells[i - startIndex].AddParagraph(dataGrid.Columns[i].Header.ToString());
                }

                // Add data for the current page
                foreach (var item in (IEnumerable)dataGrid.ItemsSource)
                {
                    MigraDoc.DocumentObjectModel.Tables.Row dataRow = table.AddRow();
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        var column = dataGrid.Columns[i];
                        TextBlock textBlock = column.GetCellContent(item) as TextBlock;

                        string cellValue = textBlock?.Text ?? string.Empty;

                        dataRow.Cells[i - startIndex].AddParagraph(cellValue);
                    }
                }
            }

            // Save the PDF document
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(outputFilePath);

            MessageBox.Show("The report completed successfully!");

            Process.Start(outputFilePath);
        }

        // Function to calculate column width based on text length
        static double CalculateColumnWidth(string text)
        {
            // You can adjust the factor to fine-tune the width calculation
            const double characterWidthFactor = 0.3;

            // Calculate the width based on the length of the text
            double width = text.Length * characterWidthFactor;

            return width;
        }


        private void btn_create_report_Click(object sender, RoutedEventArgs e)
        {
            string outputPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Reports", "output.pdf");
            GeneratePdfReport(dataGrid, outputPath);
            //GeneratePdfReport(dataGrid, outputPath);
        }

        static bool CheckDBARole()
        {
            string sql = $"SELECT COUNT(*) AS dba_role_count FROM SESSION_ROLES WHERE ROLE = 'DBA'";

            OracleCommand command = new OracleCommand();
            command.Connection = MainWindow.connection; 
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            int dbaRoleCount = Convert.ToInt32(command.ExecuteScalar());

            return dbaRoleCount > 0;
        }

        private void User_Creation_Click(object sender, RoutedEventArgs e)
        {
            if(CheckDBARole())
            {
                new UserCreationWindow().Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("You can not create new user because you are not DBA");
            }
        }

        private void btn_Home_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

    }
}
