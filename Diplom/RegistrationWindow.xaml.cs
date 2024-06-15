using DocumentFormat.OpenXml.Spreadsheet;
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
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        ApiService api = new ApiService();
        bool IsConnected = true;
        public RegistrationWindow()
        {
            InitializeComponent();
            CheckApi();
        }
        private async void CheckApi()
        {
            if (await api.CheckApiConnectionAsync() == false)
            {
                loginValidateLabel.Content = "Не удается установить связь с сервером";
                loginButton.IsEnabled = false;
                IsConnected = false;
            }
            else
            {
                loginValidateLabel.Content = "";
                loginButton.IsEnabled = true;
            }
        }

        private async void CheckApi(User user)
        {
            if (await api.CheckApiConnectionAsync() == false)
            {
                loginValidateLabel.Content = "Не удается установить связь с сервером";
                loginButton.IsEnabled = false;
            }
            else
            {
                loginValidateLabel.Content = "";
                loginButton.IsEnabled = true;
            }
        }
        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            nameValidateLabel.Content = null;
            loginValidateLabel.Content = null;
            passwordValidateLabel.Content = null;
            roleValidateLabel.Content = null;
            CheckApi();
            if(IsConnected == false)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                nameValidateLabel.Content = "Введите имя!";
                return;
            }
            if (string.IsNullOrWhiteSpace(loginTextBox.Text))
            {
                loginValidateLabel.Content = "Введите логин!";
                return;
            }
            if (string.IsNullOrWhiteSpace(passwordTextBox.Password))
            {
                passwordValidateLabel.Content = "Введите пароль!";
                return;
            }
            if (passwordTextBox.Password.Length < 8)
            {
                passwordValidateLabel.Content = "Пароль должен состоять из 8 и более символов!";
                return;
            }
            if(roleComboBox.SelectedIndex == -1)
            {
                roleValidateLabel.Content = "Выберите роль!";
                return;
            }
            User user = null;
            try
            {
                var userTask = api.GetUserAsync(loginTextBox.Text);
                user = await userTask;
            }
            catch
            {

            }
            if(user != null)
            {
                MessageBox.Show("Логин занят", "Ошибка");
                return;
            }
            user = new User
            {
                UserName = nameTextBox.Text,
                Role = roleComboBox.SelectedIndex+1,
                Login = loginTextBox.Text,
                Password = passwordTextBox.Password
            };
            await api.CreateUserAsync(user);
            MessageBox.Show("Вы зарегестрировались в системе", "Успешно",MessageBoxButton.OK);
            this.Close();
        }
    }
}
