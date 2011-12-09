﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rhit.Applications.ViewModel.Converters {
    public class ObjectToVisibilityConverter : IValueConverter {
        public ObjectToVisibilityConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null) return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}