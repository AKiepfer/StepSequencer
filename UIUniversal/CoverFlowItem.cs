// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace UIUniversal
{
    public class CoverFlowItem : ContentControl
    {
        public CoverFlowItem()
        {
            this.DefaultStyleKey = typeof(CoverFlowItem);
        }

        public event EventHandler ItemSelected;
        private FrameworkElement LayoutRoot;
        private PlaneProjection planeProjection;
        private Storyboard Animation;
        private ScaleTransform scaleTransform;
        private EasingDoubleKeyFrame rotationKeyFrame, offestZKeyFrame, scaleXKeyFrame, scaleYKeyFrame;
        private Duration duration;
        private DoubleAnimation xAnimation;
        private EasingFunctionBase easingFunction;
        private ContentControl ContentPresenter;
        private bool isAnimating;

        private double yRotation;
        public double YRotation
        {
            get
            {
                return yRotation;
            }
            set
            {
                yRotation = value;
                if (planeProjection != null)
                {
                    planeProjection.RotationY = value;
                }
            }
        }

        private double zOffset;
        public double ZOffset
        {
            get
            {
                return zOffset;
            }
            set
            {
                zOffset = value;
                if (planeProjection != null)
                {
                    planeProjection.LocalOffsetZ = value;
                }
            }
        }

        private double scale;
        public double Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                if (scaleTransform != null)
                {
                    scaleTransform.ScaleX = scale;
                    scaleTransform.ScaleY = scale;
                }
            }
        }

        private double x;
        public double X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
                Canvas.SetLeft(this, value);
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ContentPresenter = (ContentControl)GetTemplateChild("ContentPresenter");
            planeProjection = (PlaneProjection)GetTemplateChild("Rotator");
            LayoutRoot = (FrameworkElement)GetTemplateChild("LayoutRoot");

            Animation = (Storyboard)GetTemplateChild("Animation");
            Animation.Completed += Animation_Completed;

            rotationKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("rotationKeyFrame");
            offestZKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("offestZKeyFrame");
            scaleXKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("scaleXKeyFrame");
            scaleYKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("scaleYKeyFrame");
            scaleTransform = (ScaleTransform)GetTemplateChild("scaleTransform");

            planeProjection.RotationY = yRotation;
            planeProjection.LocalOffsetZ = zOffset;

            if (ContentPresenter != null)
            {
                ContentPresenter.Tapped += ContentPresenter_Tapped;
            }

            if (Animation != null)
            {
                xAnimation = new DoubleAnimation();
                Animation.Children.Add(xAnimation);
                Storyboard.SetTarget(xAnimation, this);
                Storyboard.SetTargetProperty(xAnimation, "(Canvas.Left)");
            }
        }

        void ContentPresenter_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ItemSelected != null)
                ItemSelected(this, null);
        }

        void Animation_Completed(object sender, object e)
        {
            isAnimating = false;
        }

        public void SetValues(double x, int zIndex, double r, double z, double s, Duration d, EasingFunctionBase ease, bool useAnimation)
        {
            if (useAnimation)
            {
                if (!isAnimating && Canvas.GetLeft(this) != x)
                    Canvas.SetLeft(this, this.x);

                if (rotationKeyFrame != null)
                {
                    rotationKeyFrame.Value = r;
                    offestZKeyFrame.Value = z;
                    scaleYKeyFrame.Value = s;
                    scaleXKeyFrame.Value = s;
                    xAnimation.To = x;

                    if (duration != d)
                    {
                        duration = d;
                        rotationKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
                        offestZKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
                        scaleYKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
                        scaleXKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
                        xAnimation.Duration = d;
                    }

                    if (easingFunction != ease)
                    {
                        easingFunction = ease;
                        rotationKeyFrame.EasingFunction = ease;
                        offestZKeyFrame.EasingFunction = ease;
                        scaleYKeyFrame.EasingFunction = ease;
                        scaleXKeyFrame.EasingFunction = ease;
                        xAnimation.EasingFunction = ease;
                    }

                    isAnimating = true;
                    Animation.Begin();
                    Canvas.SetZIndex(this, zIndex);
                }
            }

            this.x = x;
        }
    }

    //public class CoverFlowItem : ContentControl
    //{
    //    public CoverFlowItem()
    //    {
    //        this.DefaultStyleKey = typeof(CoverFlowItem);
    //    }

    //    public event EventHandler ItemSelected;
    //    private FrameworkElement LayoutRoot;
    //    private PlaneProjection planeProjection;
    //    private Storyboard Animation;
    //    private ScaleTransform scaleTransform;
    //    private EasingDoubleKeyFrame rotationKeyFrame, offestZKeyFrame, scaleXKeyFrame, scaleYKeyFrame;
    //    private Duration duration;
    //    private DoubleAnimation xAnimation;
    //    private EasingFunctionBase easingFunction;
    //    private ContentControl ContentPresenter;
    //    private bool isAnimating;

    //    private double yRotation;
    //    public double YRotation
    //    {
    //        get
    //        {
    //            return yRotation;
    //        }
    //        set
    //        {
    //            yRotation = value;
    //            if (planeProjection != null)
    //            {
    //                planeProjection.RotationY = value;
    //            }
    //        }
    //    }

    //    private double zOffset;
    //    public double ZOffset
    //    {
    //        get
    //        {
    //            return zOffset;
    //        }
    //        set
    //        {
    //            zOffset = value;
    //            if (planeProjection != null)
    //            {
    //                planeProjection.LocalOffsetZ = value;
    //            }
    //        }
    //    }

    //    private double scale;
    //    public double Scale
    //    {
    //        get
    //        {
    //            return scale;
    //        }
    //        set
    //        {
    //            scale = value;
    //            if (scaleTransform != null)
    //            {
    //                scaleTransform.ScaleX = scale;
    //                scaleTransform.ScaleY = scale;
    //            }
    //        }
    //    }

    //    private double x;
    //    public double X
    //    {
    //        get
    //        {
    //            return x;
    //        }
    //        set
    //        {
    //            x = value;
    //            Canvas.SetLeft(this, value);
    //        }
    //    }

    //    protected override void OnApplyTemplate()
    //    {
    //        base.OnApplyTemplate();

    //        ContentPresenter = (ContentControl)GetTemplateChild("ContentPresenter");
    //        planeProjection = (PlaneProjection)GetTemplateChild("Rotator");
    //        LayoutRoot = (FrameworkElement)GetTemplateChild("LayoutRoot");

    //        Animation = (Storyboard)GetTemplateChild("Animation");
    //        Animation.Completed += Animation_Completed;

    //        rotationKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("rotationKeyFrame");
    //        offestZKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("offestZKeyFrame");
    //        scaleXKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("scaleXKeyFrame");
    //        scaleYKeyFrame = (EasingDoubleKeyFrame)GetTemplateChild("scaleYKeyFrame");
    //        scaleTransform = (ScaleTransform)GetTemplateChild("scaleTransform");

    //        planeProjection.RotationY = yRotation;
    //        planeProjection.LocalOffsetZ = zOffset;

    //        if (ContentPresenter != null)
    //        {
    //            ContentPresenter.Tapped += ContentPresenterTapped;
    //        }

    //        if (Animation != null)
    //        {
    //            xAnimation = new DoubleAnimation();
    //            Animation.Children.Add(xAnimation);
    //            Storyboard.SetTarget(xAnimation, this);
    //            Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(Canvas.Left)").ToString());
    //        }
    //    }

    //    void ContentPresenterTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
    //    {
    //        if (ItemSelected != null)
    //            ItemSelected(this, null);
    //    }

    //    void Animation_Completed(object sender, object e)
    //    {
    //        isAnimating = false;
    //    }

    //    public void SetValues(double x, int zIndex, double r, double z, double s, Duration d, EasingFunctionBase ease, bool useAnimation)
    //    {
    //        if (useAnimation)
    //        {
    //            if (!isAnimating && Canvas.GetLeft(this) != x)
    //                Canvas.SetLeft(this, this.x);

    //            if (rotationKeyFrame != null)
    //            {
    //                rotationKeyFrame.Value = r;
    //                if (offestZKeyFrame != null)
    //                {
    //                    offestZKeyFrame.Value = z;
    //                    if (scaleYKeyFrame != null)
    //                    {
    //                        scaleYKeyFrame.Value = s;
    //                        if (scaleXKeyFrame != null)
    //                        {
    //                            scaleXKeyFrame.Value = s;
    //                            if (xAnimation != null)
    //                            {
    //                                xAnimation.To = x;

    //                                if (duration != d)
    //                                {
    //                                    duration = d;
    //                                    rotationKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
    //                                    offestZKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
    //                                    scaleYKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
    //                                    scaleXKeyFrame.KeyTime = KeyTime.FromTimeSpan(d.TimeSpan);
    //                                    xAnimation.Duration = d;
    //                                }

    //                                if (easingFunction != ease)
    //                                {
    //                                    easingFunction = ease;
    //                                    rotationKeyFrame.EasingFunction = ease;
    //                                    offestZKeyFrame.EasingFunction = ease;
    //                                    scaleYKeyFrame.EasingFunction = ease;
    //                                    scaleXKeyFrame.EasingFunction = ease;
    //                                    xAnimation.EasingFunction = ease;
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }

    //            isAnimating = true;
    //            if (Animation != null) 
    //                Animation.Begin();
    //            Canvas.SetZIndex(this, zIndex);
    //        }

    //        this.x = x;
    //    }
    //}
}
