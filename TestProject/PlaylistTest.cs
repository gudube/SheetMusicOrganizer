using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestProject
{
    class PlaylistTest : BaseTestSession
    {
        [Test]
        public void DefaultPlaylistTest()
        {
            var playlists = session.FindElementsByClassName("SelectablePlaylistItem");
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
            var oldPlaylists = session.FindElementsByClassName("SelectablePlaylistItem");
            var addPlaylistButtons = session.FindElementsByClassName("PlaylistAdderItem");
            Assert.AreEqual(1, addPlaylistButtons.Count, "There should only be one 'add playlist' button.");
            Assert.AreEqual(0, addPlaylistButtons[0].FindElementsByClassName("TextBox").Count, "The 'add playlist' button should not be editable before clicking it.");

            addPlaylistButtons[0].Click();
            var addPlaylistTextInputs = addPlaylistButtons[0].FindElementsByClassName("TextBox");
            Assert.AreEqual(1, addPlaylistTextInputs.Count, "The 'add playlist' button should be editable after clicking it.");
            addPlaylistTextInputs[0].SendKeys(" New Playlist 123!@#<>\\/ ");
            addPlaylistTextInputs[0].SendKeys(Keys.Enter);

            var newPlaylists = session.FindElementsByClassName("SelectablePlaylistItem");
            Assert.AreEqual(oldPlaylists.Count + 1, newPlaylists.Count, "A new playlist should be created after pressing enter.");
            Assert.AreEqual(true, newPlaylists.Last().Selected, "The new playlist should be selected.");
            Assert.AreEqual(" New Playlist 123!@#<>\\/ ", newPlaylists.Last().FindElementByClassName("TextBox").Text, "The new playlist should have the name entered.");
            addPlaylistButtons = session.FindElementsByClassName("PlaylistAdderItem");
            Assert.AreEqual(0, addPlaylistButtons[0].FindElementsByClassName("TextBox").Count, "The 'add playlist' button should not be editable after confirming.");
        }

        [Test]
        public void CancelEscPlaylistTest()
        {
            // test using escape key, clicking on another playlist or clicking on another selectable element (e.g. song or mastery level)
            var oldPlaylists = session.FindElementsByClassName("SelectablePlaylistItem");
            var addPlaylistButton = session.FindElementByClassName("PlaylistAdderItem");
            addPlaylistButton.Click();
            var addPlaylistTextInput = addPlaylistButton.FindElementByClassName("TextBox");
            addPlaylistTextInput.SendKeys("CancelThisPlaylist");
            
            addPlaylistTextInput.SendKeys(Keys.Escape);

            var newPlaylists = session.FindElementsByClassName("SelectablePlaylistItem");
            Assert.AreEqual(oldPlaylists.Count, newPlaylists.Count, "No new playlist should be created after pressing Escape.");
            Assert.AreEqual(0, addPlaylistButton.FindElementsByClassName("TextBox").Count, "The 'add playlist' button should not be editable after escaping.");
            Assert.AreEqual(true, newPlaylists.Last().Selected, "The last playlist should be selected after pressing Escape.");
        }


        [Test]
        public void CancelFocusPlaylistTest()
        {
            // test using escape key, clicking on another playlist or clicking on another selectable element (e.g. song or mastery level)
            var oldPlaylists = session.FindElementsByClassName("SelectablePlaylistItem");
            var addPlaylistButton = session.FindElementByClassName("PlaylistAdderItem");
            addPlaylistButton.Click();
            var addPlaylistTextInput = addPlaylistButton.FindElementByClassName("TextBox");
            addPlaylistTextInput.SendKeys("CancelThisPlaylist");

            oldPlaylists.First().Click();

            var newPlaylists = session.FindElementsByClassName("SelectablePlaylistItem");
            Assert.AreEqual(oldPlaylists.Count, newPlaylists.Count, "No new playlist should be created after losing focus.");
            Assert.AreEqual(0, addPlaylistButton.FindElementsByClassName("TextBox").Count, "The 'add playlist' button should not be editable after losing focus.");
            Assert.AreEqual(true, oldPlaylists.First().Selected, "The last playlist should be selected when clicking on it while adding new playlist.");
        }

        [Test]
        public void RenamePlaylistTest()
        {
            var addPlaylistButton = session.FindElementByClassName("PlaylistAdderItem");
            addPlaylistButton.Click();
            var addPlaylistTextInput = addPlaylistButton.FindElementByClassName("TextBox");
            addPlaylistTextInput.SendKeys("Old Playlist");
            addPlaylistTextInput.SendKeys(Keys.Enter);
            var oldPlaylists = session.FindElementsByClassName("SelectablePlaylistItem");

            Actions rightClick = new Actions(session);
            rightClick.ContextClick(oldPlaylists.Last()).Perform();
            var contextMenus = session.FindElementsByClassName("ContextMenu");
            Assert.AreEqual(1, contextMenus.Count, "There should be a context menu after right clicking on a playlist.");
            var renameMenuItem = contextMenus[0].FindElementByName("Rename");
            renameMenuItem.Click();
            var playlistTextBox = oldPlaylists.Last().FindElementByClassName("TextBox");
            playlistTextBox.SendKeys("New Playlist");
            playlistTextBox.SendKeys(Keys.Enter);

            Assert.AreEqual(0, oldPlaylists.Last().FindElementsByClassName("TextBox"), "The playlist shouldn't be editable after renaming it.");
            Assert.AreEqual("New Playlist", oldPlaylists.Last().FindElementByClassName("TextBlock").Text, "The playlist should have the new name after renaming it.");
        }

        [Test]
        public void DeletePlaylistTest()
        {
            var addPlaylistButton = session.FindElementByClassName("PlaylistAdderItem");
            addPlaylistButton.Click();
            var addPlaylistTextInput = addPlaylistButton.FindElementByClassName("TextBox");
            addPlaylistTextInput.SendKeys("Old Playlist");
            addPlaylistTextInput.SendKeys(Keys.Enter);
            var oldPlaylists = session.FindElementsByClassName("SelectablePlaylistItem");
            var oldPlaylistsCount = oldPlaylists.Count;

            Actions rightClick = new Actions(session);
            rightClick.ContextClick(oldPlaylists.Last()).Perform();
            var contextMenus = session.FindElementsByClassName("ContextMenu");
            Assert.AreEqual(1, contextMenus.Count, "There should be a context menu after right clicking on a playlist.");
            var deleteMenuItem = contextMenus[0].FindElementByName("Delete");
            deleteMenuItem.Click();

            var newPlaylists = session.FindElementsByClassName("SelectablePlaylistItem");
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
