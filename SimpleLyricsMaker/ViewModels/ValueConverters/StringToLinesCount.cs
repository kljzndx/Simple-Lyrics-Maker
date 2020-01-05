using System;
using Windows.UI.Xaml.Data;
using SimpleLyricsMaker.ViewModels.Extensions;

namespace SimpleLyricsMaker.ViewModels.ValueConverters
{
    public class StringToLinesCount : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var lines = ((string) value).ToLines();
            var linesCount = lines.Length;

            if (targetType == typeof(int))
                return linesCount;
            return linesCount.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}