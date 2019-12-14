using System;
using Windows.UI.Xaml.Data;
using SimpleLyricsMaker.ViewModels.Extensions;

namespace SimpleLyricsMaker.ViewModels.ValueConverters
{
    public class TimeSpanToLyricsTimeString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan time = (TimeSpan) value;
            return time.ToLyricsTimeString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}