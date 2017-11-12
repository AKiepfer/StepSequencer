using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FileExplorerUniversal.Control.Interop.Converters
{
    public class SelectionModeToVisibilityConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ParentSelectionModeProperty = DependencyProperty.Register("ParentSelectionMode", typeof(SelectionMode), typeof(SelectionModeToVisibilityConverter),
            new PropertyMetadata(SelectionMode.FileWithOpen));

        public SelectionMode ParentSelectionMode
        {
            get
            {
                return (SelectionMode)GetValue(ParentSelectionModeProperty);
            }
            set
            {
                SetValue(ParentSelectionModeProperty, value);
            }
        }
        
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter.ToString() == "selector")
            {
                var item = (FileExplorerItem)value;

                if (ParentSelectionMode == SelectionMode.FileWithOpen)
                {
                    if (item.IsFolder)
                    {
                        return Visibility.Collapsed;
                    }

                    return Visibility.Visible;
                }


                return Visibility.Collapsed;
            }
            else if (parameter.ToString() == "opener")
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
