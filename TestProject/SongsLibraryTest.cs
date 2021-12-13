using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.ObjectModel;

namespace TestProject
{
    [TestFixture]
    class SongsLibraryTest : BaseTestSession
    {
        private AppiumWebElement SongsGrid { get => session.FindElementByClassName("SongsGrid").FindElementByClassName("DataGrid"); }
        private ReadOnlyCollection<AppiumWebElement> Songs { get => SongsGrid.FindElementsByClassName("DataGridRow"); }

        [Test, Order(1)]
        public void DefaultSongsLibraryTest()
        {
            Assert.AreEqual(1, session.FindElementsByClassName("SongsGrid").Count, "There should be no songs in the songs library by default.");
            Assert.AreEqual(1, session.FindElementsByClassName("DataGrid").Count, "There should be no songs in the songs library by default.");

            var songs = Songs;
            Assert.AreEqual(0, songs.Count, "There should be no songs in the songs library by default.");
            var songsGrid = session.FindElementByClassName("SongsGrid");
            Assert.AreEqual(2, songsGrid.FindElementsByClassName("Button").Count, "There should be two buttons visible when the library is empty.");
            var colHeaders = SongsGrid.FindElementsByClassName("DataGridColumnHeader");
            Assert.AreEqual(11, colHeaders.Count, "There should be two 11 columns in the songs library.");
            Assert.AreEqual("#", colHeaders[0].Text, "The first column should be called '#'");
        }

        [Test]
        public void AddSongTest()
        {
            var songsGrid = SongsGrid;
            var addSongButton = songsGrid.FindElementByAccessibilityId("AddNewSongButton");

            addSongButton.Click();

            var windows = session.FindElementsByClassName("Window");
            Assert.AreEqual(1, windows.Count, "There should only be one window open when adding a new song.");
            var addSongWindow = windows[0];
            var confirmButton = addSongWindow.FindElementByName("Confirm");
            Assert.IsFalse(confirmButton.Enabled);

            addSongWindow.FindElementByAccessibilityId("selectSheetButton").Click();
            var dirInput = addSongWindow.FindElementByName("Open").FindElementByName("File name:");
            dirInput.SendKeys(TestDataDir + @"\songs\Green Day\American Idiot\Green Day - American Idiot.pdf");
            dirInput.SendKeys(Keys.Enter);
            Assert.AreEqual(TestDataDir + @"\songs\Green Day\American Idiot\Green Day - American Idiot.pdf", addSongWindow.FindElementByAccessibilityId("selectedSheetText").Text);

            addSongWindow.FindElementByAccessibilityId("selectAudio1Button").Click();
            dirInput = addSongWindow.FindElementByName("Open").FindElementByName("File name:");
            dirInput.SendKeys(TestDataDir + @"\songs\Green Day\American Idiot\Green Day - American Idiot.mp3");
            dirInput.SendKeys(Keys.Enter);
            Assert.AreEqual(TestDataDir + @"\songs\Green Day\American Idiot\Green Day - American Idiot.mp3", addSongWindow.FindElementByAccessibilityId("selectedAudio1Text").Text);

            addSongWindow.FindElementByAccessibilityId("selectAudio2Button").Click();
            dirInput = addSongWindow.FindElementByName("Open").FindElementByName("File name:");
            dirInput.SendKeys(TestDataDir + @"\songs\Green Day\American Idiot\Green Day - American Idiot (drumless).mp3");
            dirInput.SendKeys(Keys.Enter);
            Assert.AreEqual(TestDataDir + @"\songs\Green Day\American Idiot\Green Day - American Idiot (drumless).mp3", addSongWindow.FindElementByAccessibilityId("selectedAudio2Text").Text);

            Assert.IsTrue(addSongWindow.FindElementByAccessibilityId("UseAudioMDCheckBox").Selected, "Use Audio Metadata should be automatically selected.");
            // confirmButton = addSongWindow.FindElementByName("Confirm");
            Assert.IsTrue(confirmButton.Enabled);
            Assert.AreEqual(0, Songs.Count, "There should be no songs in the songs library before confirming the new song.");
            confirmButton.Click();

            var songs = Songs;
            Assert.AreEqual(1, songs.Count, "There should be one song in the library after adding a new song.");
            Assert.AreEqual(1, songs.Count, "There should be one song in the library after adding a new song.");

        }

        [Test]
        public void AddFolderTest()
        {
            Assert.Pass();
        }

    }
}
