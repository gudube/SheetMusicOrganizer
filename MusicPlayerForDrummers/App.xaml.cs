using System;
using System.Windows;
using MusicPlayerForDrummers.View;
using Serilog;

namespace MusicPlayerForDrummers
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                Log.Warning(eventArgs.Exception.ToString());
            };

            SplashScreen splash = new SplashScreen("/View/Resources/splash.png");
            splash.Show(false);

            MainWindow window = new MainWindow();
            await window.Configure();
            this.MainWindow = window;

            splash.Close(TimeSpan.FromSeconds(1));
            window.Show();
        }
    }
}
