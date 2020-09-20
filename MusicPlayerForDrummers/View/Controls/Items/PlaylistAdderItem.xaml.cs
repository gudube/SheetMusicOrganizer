using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MusicPlayerForDrummers.Model.Items;

namespace MusicPlayerForDrummers.View.Controls.Items
{
    /// <summary>
    /// Interaction logic for Adder.xaml
    /// </summary>
    public partial class PlaylistAdderItem : UserControl
    {
        public PlaylistAdderItem()
        {
            InitializeComponent();
        }

        private void AdderTextBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
                Keyboard.Focus(AdderTextBox);
        }
    }
}
