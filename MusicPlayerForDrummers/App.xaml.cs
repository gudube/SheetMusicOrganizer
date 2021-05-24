using System;
using System.Windows;
using Serilog;
using SheetMusicOrganizer.View;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace SheetMusicOrganizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Debug()
                .WriteTo.File("log.txt",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 20000000,
                    retainedFileCountLimit: 15)
                .CreateLogger();
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception? ex = (e.ExceptionObject as Exception);
                Log.Warning("Unhandled Exception Thrown!!!: {message}\n{trace}", ex?.Message, ex?.StackTrace);
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
