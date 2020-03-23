using MusicPlayerForDrummers.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MusicPlayerForDrummers.View
{
    public class MainPage : UserControl
    {
        private SwitchPageBar _leftSwitchBar;
        private SwitchPageBar _rightSwitchBar;

        public MainPage()
        {
            
        }

        protected void InitializeProperties()
        {
            _leftSwitchBar = FindName("LeftSwitchBar") as SwitchPageBar;
            _rightSwitchBar = FindName("RightSwitchBar") as SwitchPageBar;
        }

        public void SubToSwitchPageEvent(RoutedEventHandler handler)
        {
            if (_leftSwitchBar != null)
                _leftSwitchBar.SwitchPage += handler;
            if (_rightSwitchBar != null)
                _rightSwitchBar.SwitchPage += handler;
        }

        public void UnsubToSwitchPageEvent(RoutedEventHandler handler)
        {
            if (_leftSwitchBar != null)
                _leftSwitchBar.SwitchPage -= handler;
            if (_rightSwitchBar != null)
                _rightSwitchBar.SwitchPage -= handler;
        }
    }
}
