using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace FileExplorerUniversal.Control.Interop.Converters
{
    public class ExplorerTypeToIconConverter : IValueConverter
    {
        private static BitmapImage folder = new BitmapImage(new Uri("ms-appx:///FileExplorerPortable/Assets/Icons/folder.png"));
        private static BitmapImage file = new BitmapImage(new Uri("ms-appx:///FileExplorerPortable/Assets/Icons/file.png"));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isFolder = (bool)value;

            if (isFolder)
                return folder;
            else
                return file;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
