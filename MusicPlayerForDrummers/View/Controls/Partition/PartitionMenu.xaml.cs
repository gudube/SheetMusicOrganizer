﻿using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MusicPlayerForDrummers.View.Controls.Partition
{
    /// <summary>
    /// Interaction logic for PartitionMenu.xaml
    /// </summary>
    public partial class PartitionMenu : UserControl
    {
        public PartitionMenu()
        {
            InitializeComponent();
        }

        private void BindingTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && sender is TextBox textbox)
            {
                // FocusManager.SetFocusedElement(FocusManager.GetFocusScope(sender as DependencyObject), null);
                // Keyboard.ClearFocus();
                BindingExpression? be = textbox.GetBindingExpression(TextBox.TextProperty);
                be?.UpdateSource();
            }
        }
    }
}
