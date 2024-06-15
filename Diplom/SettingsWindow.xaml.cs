using Diplom.Properties;
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
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            standartFontTextBox.Text = (Settings.Default.standartFontSize).ToString();
            afterPictureTextBox.Text = (Settings.Default.afterPictureFontSize).ToString();
            firstPagesTextBox.Text = (Settings.Default.firstPagesSetting).ToString();
            switch (Settings.Default.indentation)
            {
                case 0:
                    indentationSlider.Value = 0;
                    break;
                case 7.1:
                    indentationSlider.Value = 0.25;
                    break;
                case 14.2:
                    indentationSlider.Value = 0.5;
                    break;
                case 21.3:
                    indentationSlider.Value = 0.75;
                    break;
                case 28.35:
                    indentationSlider.Value = 1;
                    break;
                case 35.45:
                    indentationSlider.Value = 1.25;
                    break;
                case 42.55:
                    indentationSlider.Value = 1.5;
                    break;
                case 49.65:
                    indentationSlider.Value = 1.75;
                    break;
                case 56.7:
                    indentationSlider.Value = 2;
                    break;
                case 63.8:
                    indentationSlider.Value = 2.25;
                    break;
                case 70.9:
                    indentationSlider.Value = 2.5;
                    break;
                case 78:
                    indentationSlider.Value = 2.75;
                    break;
                case 85.05:
                    indentationSlider.Value = 3;
                    break;
            }

        }

        private void saveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            standartFontTextBoxErrorLabel.Content = "";
            afterPictureTextBoxErrorLabel.Content = "";
            foreach (char a in standartFontTextBox.Text)
            {
                if (!char.IsDigit(a))
                {
                    standartFontTextBoxErrorLabel.Content = "Введены некорректные данные";
                    return;
                }
            }
            foreach (char a in afterPictureTextBox.Text)
            {
                if (!char.IsDigit(a))
                {
                    afterPictureTextBoxErrorLabel.Content = "Введены некорректные данные";
                    return;
                }
            }
            foreach (char a in firstPagesTextBox.Text)
            {
                if (!char.IsDigit(a))
                {
                    firstPagesTextBoxErrorLabel.Content = "Введены некорректные данные";
                    return;
                }
            }
            Settings.Default.standartFontSize = int.Parse(standartFontTextBox.Text);
            Settings.Default.afterPictureFontSize = int.Parse(afterPictureTextBox.Text);
            Settings.Default.firstPagesSetting = int.Parse(firstPagesTextBox.Text);
            switch (indentationSlider.Value)
            {
                case 0.25:
                    Settings.Default.indentation = 7.1;
                    break;
                case 0.5:
                    Settings.Default.indentation = 14.2;
                    break;
                case 0.75:
                    Settings.Default.indentation = 21.3;
                    break;
                case 1:
                    Settings.Default.indentation = 28.35;
                    break;
                case 1.25:
                    Settings.Default.indentation = 35.45;
                    break;
                case 1.5:
                    Settings.Default.indentation = 42.55;
                    break;
                case 1.75:
                    Settings.Default.indentation = 49.65;
                    break;
                case 2:
                    Settings.Default.indentation = 56.7;
                    break;
                case 2.25:
                    Settings.Default.indentation = 63.8;
                    break;
                case 2.5:
                    Settings.Default.indentation = 70.9;
                    break;
                case 2.75:
                    Settings.Default.indentation = 78;
                    break;
                case 3:
                    Settings.Default.indentation = 85.05;
                    break;
            }
            Settings.Default.Save();
            Settings.Default.Reload();
            MessageBox.Show("Настройки сохранены","Успешно");
            this.Close();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if(result == MessageBoxResult.Yes)
            {
                Properties.Settings.Default.currentUserId = 0;
                Properties.Settings.Default.currentUserRole = 0;
                Settings.Default.Save();
                Settings.Default.Reload();
                MessageBox.Show("Вы вышли из аккауна", "Успешно");
            }
            this.Close();
        }
    }
}
