using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace MusicPlayerForDrummers.View
{
    public class RatingConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            char emptyStar = '\u2606';
            char fullStar = '\u2605';
            byte rating = (byte)value;
            if (rating == 0)
                return new string(emptyStar, 5);
            else if (rating == 1)
                return fullStar + new string(emptyStar, 4);
            else if (rating <= 64)
                return new string(fullStar, 2) + new string(emptyStar, 3);
            else if (rating <= 128)
                return new string(fullStar, 3) + new string(emptyStar, 2);
            else if (rating <= 196)
                return new string(fullStar, 4) + emptyStar;
            else
                return new string(fullStar, 5);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
