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
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        ApiService api = new ApiService();
        public AuthorizationWindow()
        {
            InitializeComponent();
            loginButton.IsEnabled = false;
            registrationButton.IsEnabled = false;
            CheckApi();
        }

        private async void CheckApi()
        {
            if (await api.CheckApiConnectionAsync() == false)
            {
                loginValidateLabel.Content = "Не удается установить связь с сервером";
            }
            else
            {
                loginValidateLabel.Content = null;
                loginButton.IsEnabled = true;
                registrationButton.IsEnabled = true;
            }
        }

        private async void CheckApi(string login)
        {
            if (await api.CheckApiConnectionAsync() == false)
            {
                loginValidateLabel.Content = "Не удается установить связь с сервером";
            }
            else
            {
                var userTask = api.GetUserAsync(login);
                User user = await userTask;
                if (user == null)
                {
                    loginValidateLabel.Content = "Неверный логин!";
                    return;
                }
                if (user.Password != passwordTextBox.Password)
                {
                    passwordValidateLabel.Content = "Неверный пароль!";
                    return;
                }
                else
                {
                    Properties.Settings.Default.currentUserId = user.UserId;
                    Properties.Settings.Default.currentUserRole = (int)user.Role;
                    Properties.Settings.Default.Save();
                    Properties.Settings.Default.Reload();
                    MessageBox.Show("Вы авторизовались в системе", "Успешно", MessageBoxButton.OK);
                    this.Close();
                }
            }
        }
        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
           
            loginValidateLabel.Content = "";
            passwordValidateLabel.Content = "";
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
            CheckApi(loginTextBox.Text);
        }

        private void registrationButton_Click(object sender, RoutedEventArgs e)
        {
            var reg = new RegistrationWindow();
            this.Hide();
            reg.ShowDialog();
            this.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var main = new MainWindow();
            if (Properties.Settings.Default.currentUserId != 0)
            {
                main.Show();
                return;
            }
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите прервать авторизацию?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.No && Properties.Settings.Default.currentUserId == 0)
            {
                e.Cancel = true; // Отменить закрытие окна
            }
            else
            {
                main.Show();
                return;
            }
        }
    }
}
