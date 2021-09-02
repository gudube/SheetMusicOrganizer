using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;
using System.Linq;

namespace TestProject
{
    [TestFixture]
    class PlaylistTest : BaseTestSession
    {
        private WindowsElement PlaylistsListBox { get => session.FindElementByClassName("PlaylistsListBox"); }
        private ReadOnlyCollection<AppiumWebElement> Playlists { get => PlaylistsListBox.FindElementsByName("SheetMusicOrganizer.Model.Items.PlaylistItem"); }
        private AppiumWebElement AddPlaylistButton { get => PlaylistsListBox.FindElementByName("SheetMusicOrganizer.Model.Items.AddPlaylistItem"); }


        [Test, Order(1)]
        public void DefaultPlaylistTest()
        {
            var playlists = Playlists;
            Assert.AreEqual(1, playlists.Count, "There should only be one playlist created by default.");
            var defaultPlaylistTextBlocks = playlists[0].FindElementsByClassName("TextBlock");
            Assert.AreEqual(1, defaultPlaylistTextBlocks.Count, "There should be one text element visible inside a playlist.");
            Assert.AreEqual("All Music", defaultPlaylistTextBlocks[0].Text, "The name of the default playlist should be 'All Music'.");
            var playlistLock = playlists[0].FindElementByClassName("Image");
            Assert.AreEqual(true, playlistLock.Displayed, "The default playlist should be locked by default.");
        }

        [Test]
        public void CreatePlaylistTest()
        {
            var oldPlaylists = Playlists;
            var addPlaylistButtons = PlaylistsListBox.FindElementsByName("SheetMusicOrganizer.Model.Items.AddPlaylistItem"); ;
            Assert.AreEqual(1, addPlaylistButtons.Count, "There should only be one 'add playlist' button.");
            Assert.AreEqual(0, addPlaylistButtons[0].FindElementsByClassName("TextBox").Count, "The 'add playlist' button should not be editable before clicking it.");

            addPlaylistButtons[0].Click();
            var addPlaylistTextInputs = addPlaylistButtons[0].FindElementsByClassName("TextBox");
            Assert.AreEqual(1, addPlaylistTextInputs.Count, "The 'add playlist' button should be editable after clicking it.");
            addPlaylistTextInputs[0].SendKeys(" New Playlist 123!@#<>\\/ ");
            addPlaylistTextInputs[0].SendKeys(Keys.Enter);

            var newPlaylists = Playlists;
            Assert.AreEqual(oldPlaylists.Count + 1, newPlaylists.Count, "A new playlist should be created after pressing enter.");
            // Assert.AreEqual(true, newPlaylists.Last().GetAttribute("SelectionItem.IsSelected"));
            Assert.AreEqual(true, newPlaylists[1].Selected);
            Assert.AreEqual(" New Playlist 123!@#<>\\/ ", newPlaylists[1].FindElementByClassName("TextBlock").Text, "The new playlist should have the name entered.");
            Assert.AreEqual(0, AddPlaylistButton.FindElementsByClassName("TextBox").Count, "The 'add playlist' button should not be editable after confirming.");
        }

        [Test]
        public void CancelEscPlaylistTest()
        {
            // test using escape key, clicking on another playlist or clicking on another selectable element (e.g. song or mastery level)
            var oldPlaylists = Playlists;
            var addPlaylistButton = AddPlaylistButton;
            addPlaylistButton.Click();
            var addPlaylistTextInput = addPlaylistButton.FindElementByClassName("TextBox");
            addPlaylistTextInput.SendKeys("CancelThisPlaylist");
            
            addPlaylistTextInput.SendKeys(Keys.Escape);

            var newPlaylists = Playlists;
            Assert.AreEqual(oldPlaylists.Count, newPlaylists.Count, "No new playlist should be created after pressing Escape.");
            Assert.AreEqual(0, addPlaylistButton.FindElementsByClassName("TextBox").Count, "The 'add playlist' button should not be editable after escaping.");
            Assert.AreEqual(true, newPlaylists.Last().Selected, "The last playlist should be selected after pressing Escape.");
        }


        [Test]
        public void CancelFocusPlaylistTest()
        {
            // test using escape key, clicking on another playlist or clicking on another selectable element (e.g. song or mastery level)
            var oldPlaylists = Playlists;
            var addPlaylistButton = AddPlaylistButton;
            addPlaylistButton.Click();
            var addPlaylistTextInput = addPlaylistButton.FindElementByClassName("TextBox");
            addPlaylistTextInput.SendKeys("CancelThisPlaylist");

            oldPlaylists.First().Click();

            var newPlaylists = Playlists;
            Assert.AreEqual(oldPlaylists.Count, newPlaylists.Count, "No new playlist should be created after losing focus.");
            Assert.AreEqual(0, addPlaylistButton.FindElementsByClassName("TextBox").Count, "The 'add playlist' button should not be editable after losing focus.");
            Assert.AreEqual(true, oldPlaylists.First().Selected, "The last playlist should be selected when clicking on it while adding new playlist.");
        }

        [Test]
        public void RenamePlaylistTest()
        {
            var addPlaylistButton = AddPlaylistButton;
            addPlaylistButton.Click();
            var addPlaylistTextInput = addPlaylistButton.FindElementByClassName("TextBox");
            addPlaylistTextInput.SendKeys("Old Playlist");
            addPlaylistTextInput.SendKeys(Keys.Enter);
            var oldPlaylists = Playlists;

            Actions rightClick = new Actions(session);
            rightClick.ContextClick(oldPlaylists.Last()).Perform();
            var contextMenus = session.FindElementsByClassName("ContextMenu");
            Assert.AreEqual(1, contextMenus.Count, "There should be a context menu after right clicking on a playlist.");
            var renameMenuItem = contextMenus[0].FindElementByName("Rename");
            renameMenuItem.Click();
            var playlistTextBox = oldPlaylists.Last().FindElementByClassName("TextBox");
            playlistTextBox.SendKeys("New Playlist");
            playlistTextBox.SendKeys(Keys.Enter);

            var newPlaylists = Playlists;
            Assert.AreEqual(0, newPlaylists.Last().FindElementsByClassName("TextBox").Count, "The playlist shouldn't be editable after renaming it.");
            Assert.AreEqual("New Playlist", newPlaylists.Last().FindElementByClassName("TextBlock").Text, "The playlist should have the new name after renaming it.");
        }

        [Test]
        public void DeletePlaylistTest()
        {
            var addPlaylistButton = AddPlaylistButton;
            addPlaylistButton.Click();
            var addPlaylistTextInput = addPlaylistButton.FindElementByClassName("TextBox");
            addPlaylistTextInput.SendKeys("Old Playlist");
            addPlaylistTextInput.SendKeys(Keys.Enter);
            var oldPlaylists = Playlists;
            var oldPlaylistsCount = oldPlaylists.Count;

            Actions rightClick = new Actions(session);
            rightClick.ContextClick(oldPlaylists.Last()).Perform();
            var contextMenus = session.FindElementsByClassName("ContextMenu");
            Assert.AreEqual(1, contextMenus.Count, "There should be a context menu after right clicking on a playlist.");
            var deleteMenuItem = contextMenus[0].FindElementByName("Delete");
            deleteMenuItem.Click();

            var newPlaylists = Playlists;
            Assert.AreEqual(oldPlaylistsCount - 1, newPlaylists.Count, "There should be one less playlist after deleting one.");
            Assert.AreEqual(true, newPlaylists.Last().Selected, "The playlist on top of the deleted one should be selected.");
        }

        [Test]
        public void PlayingPlaylistTest()
        {
            Assert.Pass();
        }

    }
}
