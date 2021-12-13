using System;
using System.IO;
using System.Windows;
using Serilog;
using SheetMusicOrganizer.View;
using Windows.UI;
using Windows.UI.ViewManagement;
using SheetMusicOrganizer.View.Tools;
using Microsoft.Data.Sqlite;

namespace SheetMusicOrganizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NTA1NDA1QDMxMzkyZTMyMmUzME43c0xPRGQvVG8zRHhnd3hsb0xjU205TW4relhXRldsVHFrRktZaGYrbXM9");
        }

        private async void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.UserDir))
                Settings.Default.UserDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Settings.Default.ApplicationName);

            bool showSplashScreen = true;
            string[] args = Environment.GetCommandLineArgs();
            foreach(string arg in args)
            {
                if(arg == "hardReset")
                    Settings.Default.Reset();
                if (arg.StartsWith("overrideUserDir="))
                    Settings.Default.UserDir = arg.Substring("overrideUserDir=".Length);
                if (arg == "noSplash")
                    showSplashScreen = false;
            }
            Settings.Default.Save();

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

            SplashScreen? splash = null;
            if (showSplashScreen)
            {
                splash = new SplashScreen("/View/Resources/splash.png");
                splash?.Show(false);
            }

            try
            {
                MainWindow window = new MainWindow();

                await window.Configure();

                this.MainWindow = window;

                if (showSplashScreen)
                {
                    splash?.Close(TimeSpan.FromSeconds(1));
                }
                window.Show();
            }
            catch (SqliteException ex)
            {
                WindowManager.OpenErrorAsMainWindow(ex, $"There was an error when trying to create/open the database from : {Settings.Default.RecentDBs[0]}");
                if (showSplashScreen)
                {
                    splash?.Close(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}
