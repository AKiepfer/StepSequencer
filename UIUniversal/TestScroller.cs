using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace UIUniversal
{
    public class TestScroller : RangeBase
    {
        private double start;
        private Image _rotateImage;
        private Image _rotateIndicator;

        public TestScroller()
        {
            ManipulationMode = ManipulationModes.TranslateY; //ManipulationModes.TranslateX | ;

            this.LargeChange = 0.021;
            this.SmallChange = 0.010;
        }

        static TestScroller()
        {
            
        }

        private T FindVisualChildByType<T>(DependencyObject element, String name) where T : class    
        {
            var frameworkElement = element as FrameworkElement;
            if (frameworkElement != null && (element is T && frameworkElement.Name == name))
                return element as T;
            int childcount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childcount; i++)
            {
                T childElement = FindVisualChildByType<T>(VisualTreeHelper.GetChild(element, i), name);
                if (childElement != null)
                    return childElement;
            }
            return null;
        }


        protected override void OnManipulationCompleted(Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            base.OnManipulationCompleted(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
        {
            double added = 0.0;

            if (e.Delta.Translation.Y < 0 )
            {
                added = LargeChange;
            }

            if (e.Delta.Translation.Y > 0)
            {
                added = -LargeChange;
            }

            if (e.Delta.Translation.X > 0)
            {
                added = -SmallChange;
            }

            if (e.Delta.Translation.X < 0)
            {
                added = SmallChange;
            }

            e.Handled = true;

            this.Value = this.Value + added;
            
            base.OnManipulationDelta(e);
            
        }

        protected override void OnApplyTemplate()
        {
            RotateImages();
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            RotateImages();

            base.OnValueChanged(oldValue, newValue);
        }

        private void RotateImages()
        {
            if (Math.Abs(Maximum - Minimum) <= 0.0)
            {
                return;
            }

            double steps = 360/(Maximum - Minimum);

            var rotating = new RotateTransform
                           {
                               Angle =
                                   (this.Value*steps) -
                                   (steps*Minimum)
                           };

            _rotateImage = _rotateImage ?? (_rotateImage = FindVisualChildByType<Image>(this, "PART_RotateImage"));

            if (_rotateImage != null)
            {
                _rotateImage.RenderTransformOrigin = new Point(0.5, 0.5);
                _rotateImage.RenderTransform = rotating;
            }

            _rotateIndicator = _rotateIndicator ?? (_rotateIndicator = FindVisualChildByType<Image>(this, "PART_RotateIndicator"));

            if (_rotateIndicator != null)
            {
                _rotateIndicator.RenderTransformOrigin = new Point(0.5, 0.5);
                _rotateIndicator.RenderTransform = rotating;
            }
        }
    }
}

    
