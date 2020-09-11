using System;
using System.Windows;
using MusicPlayerForDrummers.View;

namespace MusicPlayerForDrummers
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
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
