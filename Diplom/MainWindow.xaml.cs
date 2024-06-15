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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using Microsoft.Win32;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;
using Document = Microsoft.Office.Interop.Word.Document;
using Paragraph = Microsoft.Office.Interop.Word.Paragraph;
using DocumentFormat.OpenXml.Bibliography;
using Section = Microsoft.Office.Interop.Word.Section;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        AverageDocument doc = new AverageDocument();
        ApiService api = new ApiService();
        bool isConnected = false;

        public MainWindow()
        {
            InitializeComponent();
            authorizationStackPanel.Visibility = Visibility.Collapsed;
            profileMenuItem.IsEnabled = false;
            uploadFileCheckBox.IsChecked = false;
            uploadFileCheckBox.IsEnabled = false;
            CheckApi();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string filePath in files)
                {
                    if (System.IO.Path.GetExtension(filePath).ToLower() == ".docx" || System.IO.Path.GetExtension(filePath).ToLower() == ".doc")
                    {
                        documentNameLabel.Text = System.IO.Path.GetFileName(filePath);
                        doc.filePath = filePath;
                        startExamButton.IsEnabled = true;
                        MessageBox.Show($"Файл {System.IO.Path.GetFileName(filePath)} готов к проверке");
                    }
                    else
                    {
                        MessageBox.Show($"Файл {System.IO.Path.GetFileName(filePath)} не является документом Word.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Hand);
                    }
                }
            }
        }

        private async void ProcessWordDocument(string filePath)
        {
            Application application = new Application();
            Document document = new Document();
            try
            {
                document = application.Documents.Open(filePath, ConfirmConversions: false, ReadOnly: true, AddToRecentFiles: false, Revert:false , Visible: false);
            }
            catch
            {
                MessageBox.Show("Произошла ошибка открытия файла", "Ошибка");
                application.Quit();
                return;
            }
            Paragraph prevPara = null;
            List<DocError> errors = new List<DocError>();
            int progress = 1;
            foreach (Paragraph para in document.Paragraphs)
            {

                await System.Threading.Tasks.Task.Run(() => ProcessParagraph(document.Paragraphs.Count, progress));
                progress++;
                bool hasPicture = false;
                bool isInTable = (bool)para.Range.Information[WdInformation.wdWithInTable];
                int pageNumber = para.Range.get_Information(WdInformation.wdActiveEndAdjustedPageNumber);
                int pageIndex = para.Range.Information[WdInformation.wdActiveEndPageNumber];
                if (pageIndex <= Properties.Settings.Default.firstPagesSetting)
                {
                    continue;
                }
                if (para.Range.Text == "\r")
                {
                    continue;
                }
                if (para.Range.Text == "\r\a")
                {
                    continue;
                }
                if (para.Range.Text == "\a")
                {
                    continue;
                }
                if (para.Range.Text == "/\r")
                {
                    prevPara = para;
                    continue;
                }

                if (prevPara != null && prevPara.Range.Text == "/\r")
                {
                    hasPicture = true;
                    prevPara = null;
                }

                foreach (Range word in para.Range.Words)
                {
                    if (word.Text == "\r")
                    {
                        continue;
                    }
                    if (word.Text == "\r\a")
                    {
                        continue;
                    }
                    if (word.Text == "\a")
                    {
                        continue;
                    }
                    if (word.Font.Size != Diplom.Properties.Settings.Default.standartFontSize && !(isInTable == true && word.Font.Size == 12) && hasPicture == false)
                    {
                        var error = new DocError();
                        error.text = word.Text;
                        error.page = pageNumber;
                        error.errorDescription = " Ошибка: Размер шрифта = " + word.Font.Size;
                        errors.Add(error);
                    }
                    else if(hasPicture == true && word.Font.Size != Properties.Settings.Default.afterPictureFontSize)
                    {
                        var error = new DocError();
                        error.text = word.Text;
                        error.page = pageNumber;
                        error.errorDescription = " Ошибка: Размер шрифта после картинки = " + word.Font.Size;
                        errors.Add(error);
                    }
                    if (word.Font.Name != Diplom.Properties.Settings.Default.standartFontName)
                    {
                        string f = word.Font.Name;
                        var error = new DocError();
                        error.text = word.Text;
                        error.page = pageNumber;
                        error.errorDescription = " Ошибка: Шрифт = " + word.Font.Name;
                        errors.Add(error);
                    }
                }
                if(para.Alignment != WdParagraphAlignment.wdAlignParagraphCenter && para.Alignment != WdParagraphAlignment.wdAlignParagraphJustify)
                {
                    var error = new DocError();
                    error.text = para.Range.Text;
                    error.page = pageNumber;
                    error.errorDescription = "Найден параграф с неверным выравниванием";
                    errors.Add(error);
                }
                if((para.Alignment == WdParagraphAlignment.wdAlignParagraphCenter || para.Alignment == WdParagraphAlignment.wdAlignParagraphRight) && !(para.LeftIndent == 0 && para.FirstLineIndent == 0))
                {
                    var error = new DocError();
                    error.text = para.Range.Text;
                    error.page = pageNumber;
                    error.errorDescription = "Найден параграф с неверным отступом на странице";
                    errors.Add(error);
                }
                double epsilon = 0.1;
                if ((para.Alignment == WdParagraphAlignment.wdAlignParagraphJustify || para.Alignment == WdParagraphAlignment.wdAlignParagraphLeft) && (!(Math.Abs(Convert.ToDouble(para.FirstLineIndent) - Properties.Settings.Default.indentation) < epsilon)) && isInTable==false)
                {
                    var error = new DocError();
                    error.text = para.Range.Text;
                    error.page = pageNumber;
                    error.errorDescription = "Найден параграф с неверным отступом на странице";
                    errors.Add(error);
                }
                hasPicture = false;
            }
            foreach (Section section in document.Sections)
            {
                // Проверяем наличие нижнего колонтитула на текущей странице
                if (section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Exists)
                {
                    // Извлекаем текст из нижнего колонтитула текущей страницы
                    string footerText = section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Text;

                    // Проверяем, содержит ли колонтитул номер страницы
                    if (footerText != section.Index.ToString()+"\r" && footerText != section.Index.ToString() + "\r\r")
                    {
                        var error = new DocError();
                        error.page = section.Index;
                        error.errorDescription = "Номер страницы не указан в нижнем колонтитуле на странице";
                        errors.Add(error);
                    }

                    // Получаем размер и шрифт текста в колонтитуле
                    float fontSize = section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Font.Size;
                    string fontName = section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary].Range.Font.Name;
                }
                else
                {
                    var error = new DocError();
                    error.page = section.Index;
                    error.errorDescription = "Документ на странице не имеет нижнего колонтитула.";
                    errors.Add(error);
                }
                
            }
            errorsListView.ItemsSource = errors;

            try
            {
                if (Properties.Settings.Default.currentUserId != 0 && Properties.Settings.Default.currentUserRole == 1 && uploadFileCheckBox.IsChecked == true && isConnected == true)
                {
                    string fileName = System.IO.Path.GetFileName(filePath);
                    byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                    File file = new File
                    {
                        FileName = fileName,
                        FilePath = filePath,
                        Iduser = Properties.Settings.Default.currentUserId,
                        FileData = fileData,
                        CreationDate = DateTime.Now
                    };
                    ApiService api = new ApiService();
                    await api.PostFile(file);
                    MessageBox.Show("Файл добавлен", "Успешно");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            application.Quit();
            

        }

        private bool IsDocFile(string filePath)
        {
            // Проверяем, что расширение файла соответствует .docx или .doc
            string extension = System.IO.Path.GetExtension(filePath).ToLower();
            return extension == ".docx" || extension == ".doc";
        }

        private void startExamButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessWordDocument(doc.filePath);
        }

        private void chooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Документы Word (*.doc;*.docx)|*.doc;*.docx|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                if (System.IO.Path.GetExtension(filePath).ToLower() == ".docx" || System.IO.Path.GetExtension(filePath).ToLower() == ".doc")
                {
                    documentNameLabel.Text = System.IO.Path.GetFileName(filePath);
                    doc.filePath = filePath;
                    startExamButton.IsEnabled = true;
                    MessageBox.Show($"Файл {System.IO.Path.GetFileName(filePath)} готов к проверке");
                }
                else
                {
                    MessageBox.Show($"Файл {System.IO.Path.GetFileName(filePath)} не является документом Word.","Ошибка",MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            }
        }

        private void settingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow();
            settings.ShowDialog();
            CheckApi();
        }

        private void authButton_Click(object sender, RoutedEventArgs e)
        {
            var auth = new AuthorizationWindow();
            this.Close();
            auth.Show();
            
        }

        private void ProcessParagraph(int paragraphsCount, int currentParagraphNumber)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                progressProgressBar.Value = (double)currentParagraphNumber / paragraphsCount * 100;
            });
        }

        private async void CheckApi()
        {
            if (await api.CheckApiConnectionAsync() == false)
            {
                loginValidateLabel.Text = "Не удается установить связь с сервером";
                return;
                
            }
            if(Properties.Settings.Default.currentUserRole == 0)
            {
                authorizationStackPanel.Visibility = Visibility.Visible;
                profileMenuItem.IsEnabled = false;
                return;
            }
            if(Properties.Settings.Default.currentUserRole == 1)
            {
                uploadFileCheckBox.IsChecked = true;
                uploadFileCheckBox.IsEnabled = true;
            }
            profileMenuItem.IsEnabled = true;
            isConnected = true;
            loginValidateLabel.Text = null;
        }

        private async void CheckApiFiles()
        {
            if (await api.CheckApiConnectionAsync() == false)
            {
                loginValidateLabel.Text = "Не удается установить связь с сервером";
                return;

            }
            if (Properties.Settings.Default.currentUserId == 0)
            {
                authorizationStackPanel.Visibility = Visibility.Visible;
                profileMenuItem.IsEnabled = false;
                return;
            }
            else if (Properties.Settings.Default.currentUserId == 1)
            {
                uploadFileCheckBox.IsChecked = true;
                uploadFileCheckBox.IsEnabled = true;
            }
            if (Properties.Settings.Default.currentUserRole == 1)
            {
                var files = new FilesWindow();
                this.Close();
                files.Show();
            }
            else if (Properties.Settings.Default.currentUserRole == 2)
            {
                var files = new TeacherFilesWindow();
                this.Close();
                files.Show();
            }
        }

        private void profileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CheckApiFiles();
        }
    }
}
