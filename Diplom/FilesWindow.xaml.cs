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
using Diplom.Models;
using Microsoft.Win32;
namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для FilesWindow.xaml
    /// </summary>
    public partial class FilesWindow : Window
    {
        ApiService api = new ApiService();
        List<FileImport> filesList = new List<FileImport>();
        private bool _isClosedByButton;
        public FilesWindow()
        {
            InitializeComponent();
            GetFilesByUserId(Properties.Settings.Default.currentUserId);
        }

        public async void GetFilesByUserId(int id)
        {
            try
            {
                var apifiles = await api.GetFilesByUserIdAsync(id);
                filesList = apifiles.ToList();
                FilesItemsControl.ItemsSource = filesList;
            }
            catch(Exception ex)
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
                DateTime? dateFilter = creationDateDatePicker.SelectedDate;

                // Фильтруем данные
                var filteredFiles = filesList.Where(file =>
                                    (string.IsNullOrEmpty(loginFilter) || (file.UserLogin != null && file.FileName.ToLower().Contains(loginFilter))) &&
                                    (!dateFilter.HasValue || file.CreationDate.Date == dateFilter.Value.Date)
                                    ).ToList();

                // Обновляем привязку данных
                FilesItemsControl.ItemsSource = filteredFiles;
            }
            catch
            {

            }
        }

        private async void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            var document = (sender as Button).DataContext as FileImport;
            MessageBoxResult agree = MessageBox.Show("Вы уверены что хотите безвозвратно удалить файл с сервера?","Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (document != null && agree == MessageBoxResult.Yes)
            {
                await api.DeleteFileAsync(document.FileId);
                var file = filesList.FirstOrDefault(f => f.FileId == document.FileId);
                filesList.Remove(file);
                FilesItemsControl.ItemsSource = filesList;
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
