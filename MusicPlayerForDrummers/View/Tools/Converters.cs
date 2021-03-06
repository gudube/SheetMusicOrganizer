﻿using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Serilog;

namespace MusicPlayerForDrummers.View.Tools
{
    public class RatingConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            char emptyStar = '\u2606';
            char fullStar = '\u2605';
            int rating;
            if (value != null)
                rating = System.Convert.ToInt32(value);
            else
            {
                rating = 0;
                Log.Warning("Received null value to convert to a rating (int) in RatingConverter");
            }

            return new string(fullStar, rating) + new string(emptyStar, 5 - rating);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class HexColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if(new BrushConverter().ConvertFrom(value) is SolidColorBrush brush)
                return brush;
            
            Log.Error("Could not convert HEX to color: ", value);
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
            /*
            if(value is SolidColorBrush color) 
                return color.Color.ToString();

            Log.Warning("Could not convert value {value} to SolidColorBrush");*/
        }
    }

    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace((string) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringsNotEmptyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
                if (value is string text && string.IsNullOrWhiteSpace(text))
                    return false;
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DirFilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetFileName((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FloatPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float num)
                return num * 100;

            Log.Warning("Could not convert value {value} as float to transform to /100 percentage", value);
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double num)
                return num / 100;

            Log.Warning("Could not convert value {value} as double to transform to /1.0 percentage", value);
            return 0.0;
        }
    }

    public class CrossMultiplicationConverter : IMultiValueConverter
    {
        //input values: a,b,d so that (a/b)*(?/d)
        //ex: posSong, lengthSong, lengthPixels
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (Double.TryParse(System.Convert.ToString(values[0]), out var a) && Double.IsFinite(a) &&
                Double.TryParse(System.Convert.ToString(values[1]), out var b) && Double.IsFinite(b) && Math.Abs(b) > 0.0000001 &&
                Double.TryParse(System.Convert.ToString(values[2]), out var d) && Double.IsFinite(d))
                return d * a / b;
            
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SubstractionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double value = 0.0;
            if (Double.TryParse(System.Convert.ToString(values[0]), out var a) && Double.IsFinite(a) &&
                Double.TryParse(System.Convert.ToString(values[1]), out var b) && Double.IsFinite(b))
                value = a - b;

            if (value < 0)
                return 0.0;
           
            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
