using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using SimpleLyricsMaker.ViewModels.Settings;

namespace SimpleLyricsMaker.ViewModels.ValueConverters
{
    public class BilingualStringToNormalString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string str = (string) value;
            var lines = str.Split(EditViewSettingProperties.Current.SplitSymbol).Select(s => s.Trim()).ToList();
            if (lines.Count == 1)
                return str;
            return String.Join(Environment.NewLine, lines);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}