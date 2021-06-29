using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TestProject
{
    public class BaseTestSession
    {
        protected static WindowsDriver<WindowsElement> session;

        private const string AppId = @"D:\Documents\repos\SheetMusicOrganizer\TestProject\project\MusicPlayerForDrummers.exe";
        private const string TempDataLocation = @"D:\Documents\repos\SheetMusicOrganizer\TestProject\data\temp";
        private const string WinAppDriverLocation = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";

        [OneTimeSetUp]
        public void Setup()
        {
            if (session == null)
            {
                // cleanup temp data directory
                DirectoryInfo di = new DirectoryInfo(TempDataLocation);
                Assert.IsTrue(di.Exists);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();

                // start application
                AppiumOptions options = new AppiumOptions();
                options.AddAdditionalCapability("app", AppId);
                options.AddAdditionalCapability("deviceName", "WindowsPC");
                options.AddAdditionalCapability("platformName", "Windows");
                options.AddAdditionalCapability("appArguments", "noSplash hardReset overrideUserDir=" + TempDataLocation);
                try
                {
                    session = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);
                } catch
                {
                    if(File.Exists(WinAppDriverLocation))
                    {
                        ProcessStartInfo procInfo = new ProcessStartInfo(WinAppDriverLocation);
                        procInfo.CreateNoWindow = false;
                        procInfo.UseShellExecute = true;
                        procInfo.WindowStyle = ProcessWindowStyle.Minimized;
                        Process procRun = Process.Start(procInfo);
                        Thread.Sleep(2000); //wait for the WinAppDriver app to start
                        session = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);
                    }
                }
                Assert.IsNotNull(session);
                Assert.IsNotNull(session.SessionId);
                session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);
                session.Manage().Window.Maximize();
                // Thread.Sleep(5000); //wait for the app to start
            }
        }

        [Test]
        public void BaseTest()
        {
            Assert.Pass();
        }

        [OneTimeTearDown]
        public void Close()
        {
            //session.CloseApp();
        }
    }
}