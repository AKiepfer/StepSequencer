using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FileExplorerUniversal.Control.Interop.Converters
{
    public class SelectionModeToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SelectionMode mode = (SelectionMode)value;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
