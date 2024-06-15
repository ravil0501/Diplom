using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "Проверка файлов";
            bool createdNew;
            base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Приложение уже запущено.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
                return;
            }

            // Remove the redundant second call to base.OnStartup(e)
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                    _mutex.Dispose();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
            }
            finally
            {
                base.OnExit(e);
            }
        }
    }
}
