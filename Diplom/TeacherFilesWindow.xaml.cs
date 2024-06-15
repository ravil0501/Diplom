using Diplom.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для TeacherFilesWindow.xaml
    /// </summary>
    public partial class TeacherFilesWindow : Window
    {
        ApiService api = new ApiService();
        List<FileImport> filesList = new List<FileImport>();
        private bool _isClosedByButton;
        public TeacherFilesWindow()
        {
            InitializeComponent();
            GetFiles();
        }

        public async void GetFiles()
        {
            try
            {
                var apifiles = await api.GetFiles();
                filesList = apifiles.ToList();
                FilesItemsControl.ItemsSource = filesList;
            }
            catch
            {

            }
        }

        private void downloadButtonClick_Click(object sender, RoutedEventArgs e)
        {
            var document = (sender as Button).DataContext as FileImport;

            if (document != null && document.FileData != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = document.FileName;
                saveFileDialog.Filter = "Word Document (*.docx)|*.docx";

                if (saveFileDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllBytes(saveFileDialog.FileName, document.FileData);
                    MessageBox.Show("Файл успешно сохранен.", "Сохранение файла", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Данные файла отсутствуют.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void applyFilterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string loginFilter = loginTextBox.Text.ToLower();
                int? groupFilter = groupComboBox.SelectedItem as int?;
                DateTime? dateFilter = creationDateDatePicker.SelectedDate;

                // Фильтруем данные
                var filteredFiles = filesList.Where(file =>
                                    (string.IsNullOrEmpty(loginFilter) || (file.UserLogin != null && file.UserLogin.ToLower().Contains(loginFilter))) &&
                                    (!groupFilter.HasValue || file.GroupNumber == groupFilter.Value) &&
                                    (!dateFilter.HasValue || file.CreationDate.Date == dateFilter.Value.Date)
                                    ).ToList();

                // Обновляем привязку данных
                FilesItemsControl.ItemsSource = filteredFiles;
            }
            catch
            {

            }
        }
        private void disableFilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilesItemsControl.ItemsSource = filesList;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isClosedByButton)
            {
                var main = new MainWindow();
                main.Show();
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            _isClosedByButton = true;
            var main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
