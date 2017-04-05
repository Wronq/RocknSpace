using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RocknSpace
{
    public class DivideBy2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? width = value as double?;

            return width.HasValue ? width.Value / 2 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? width = value as double?;

            return width.HasValue ? width.Value / 2 : 0;
        }
    }

    public class SizeToRotatedRectConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            double? width = value[0] as double?;
            if (!width.HasValue) return null;

            double? height = value[1] as double?;
            if (!height.HasValue) return null;

            string offset = parameter as string;
            if (offset == null) return null;
            
            float len = (float)Math.Sqrt(width.Value * width.Value + height.Value * height.Value) - (float)double.Parse(offset) * 2;
            return new Rect(width.Value/2 - len/2, -2500, len, 5000);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToOnOffConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? val = value as bool?;

            return val.HasValue ? (val.Value ? "on" : "off") : "off";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value as string;

            return val.ToLower() == "on" ? true : false;
        }
    }

    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
