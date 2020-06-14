﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MusicPlayerForDrummers.View.Tools
{
    public class BindingHelper
    {
        /// <summary>
        /// To block infinite loop E.g. block updates list to collection when collection is updating list.
        /// </summary>
        private bool _isUpdating = false;

        /// <summary>
        /// Updates a List to copy changes from an ObservableCollection_CollectionChanged event
        /// E.g. Binding a ViewModel ObservableCollection to a ViewList :
        /// ((LibraryVM)DataContext).Session.SelectedMasteryLevels.CollectionChanged += (sender, e) => BindingHelper.ObservableCollectionChanged<MasteryItem>(MainListBox.SelectedItems, sender, e);
        /// </summary>
        /// <typeparam name="T">Type of items in the list/collection</typeparam>
        /// <param name="listToUpdate">List to update (e.g. in the view)</param>
        /// <param name="sender">Sender parameter from the CollectionChanged Event</param>
        /// <param name="e">EventArgs from the CollectionChanged Event</param>
        private void ObservableCollectionChanged<T>(IList listToUpdate, object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isUpdating)
                return;
            _isUpdating = true;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (T item in e.NewItems)
                        //if (!listToUpdate.Contains(item))
                            listToUpdate.Add(item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                        //if (listToUpdate.Contains(item))
                            listToUpdate.Remove(item);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    listToUpdate.Clear();
                    foreach (T item in sender as ObservableCollection<T>)
                        listToUpdate.Add(item);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (T item in e.OldItems)
                        //if (listToUpdate.Contains(item))
                            listToUpdate.Remove(item);
                    foreach (T item in e.NewItems)
                        //if (!listToUpdate.Contains(item))
                            listToUpdate.Add(item);
                    break;
                default: break;
            }
            _isUpdating = false;
        }

        /// <summary>
        /// Updates an ObservableCollection to copy the changes made to a List
        /// E.g. binding a View List to a Viewmodel ObservableCollection
        /// MainListBox.SelectionChanged += (sender, e) => BindingHelper.ListChanged(((LibraryVM)DataContext).Session.SelectedMasteryLevels, sender, e);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionToUpdate"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListChanged<T>(ObservableCollection<T> collectionToUpdate, object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating)
                return;
            _isUpdating = true;
            foreach (T item in e.AddedItems)
                //if(!collectionToUpdate.Contains(item))
                    collectionToUpdate.Add(item);
            foreach (T item in e.RemovedItems)
                //if(collectionToUpdate.Contains(item))
                    collectionToUpdate.Remove(item);
            _isUpdating = false;
        }

        /// <summary>
        /// Add the event handlers for view list -> vm collection and vice versa.
        /// E.g. DataContextChanged += BindingHelper.BidirectionalLink(() => DataContext, () => ((LibraryVM)DataContext).Session.SelectedMasteryLevels, MainListBox, MainListBox.SelectedItems);
        /// </summary>
        /// <typeparam name="T">Type of items in list/collection</typeparam>
        /// <param name="dataContext">function to get the datacontext once it's changed</param>
        /// <param name="vmCollection">function to get the collection from the viewmodel</param>
        /// <param name="viewList">list from the view</param>
        /// <param name="viewItems">binded items from the list of the view (e.g. selectedItems)</param>
        /// <returns></returns>
        public static DependencyPropertyChangedEventHandler BidirectionalLink<T>(Func<object> dataContext, Func<ObservableCollection<T>> vmCollection, Selector viewList, IList viewItems)
        {
            var instance = new BindingHelper();
            return (sender, args) =>
            {
                if(dataContext() != null)
                    vmCollection().CollectionChanged += (sender, e) => instance.ObservableCollectionChanged<T>(viewItems, sender, e);
                viewList.SelectionChanged += (sender, e) => { if (dataContext() != null) instance.ListChanged(vmCollection(), sender, e); };
            };
        }
    }
}