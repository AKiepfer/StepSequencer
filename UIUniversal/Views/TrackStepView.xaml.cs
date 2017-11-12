using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using FileExplorerUniversal;
using FileExplorerUniversal.Control.Interop;
using ManagedAudioEngineUniversal.Core;
using UIUniversal.ViewModels;

namespace UIUniversal.Views
{
    public partial class TrackStepView 
    {
        private readonly List<Image> TriggerImages;
        private List<CheckBox> CheckBoxes;
        private  FileExplorer Explorer;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

         
        private static BitmapImage OnImage = new BitmapImage(new Uri(@"ms-appx:///UIUniversal/Assets/Blinking_LED/On.png"));
        private static BitmapImage OffImage = new BitmapImage(new Uri(@"ms-appx:///UIUniversal/Assets/Blinking_LED/Off.png"));
        private CoreDispatcher _coreDispatcher;

        public TrackStepView()
        {
            this.DataContextChanged += OnDataContextChanged;
            
            InitializeComponent();

            _coreDispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            CheckBoxes = Container.GetDescendantsOfType<CheckBox>()
                .Where(box => box.Name.Contains("Play"))
                .ToList();

            for (int i = 0;
                i < CheckBoxes.Count;
                i++)
            {
                int i1 = i;
                CheckBoxes[i1].Checked +=
                    (sender, args) =>
                    {
                        var trackStepViewModel = DataContext as TrackStepViewModel;

                        if (trackStepViewModel != null)
                        {
                            var isChecked = CheckBoxes[i1].IsChecked;
                            trackStepViewModel.SetAt(i1, isChecked != null && isChecked.Value);
                        }
                    };

                (CheckBoxes[i1]).Unchecked +=
                    (sender, args) =>
                    {
                        var trackStepViewModel = DataContext as TrackStepViewModel;
                        if (trackStepViewModel != null)
                        {
                            var isChecked = CheckBoxes[i1].IsChecked;
                            trackStepViewModel.SetAt(i1, isChecked != null && isChecked.Value);
                        }
                    };
            }

            TriggerImages = Container.GetDescendantsOfType<Image>()
                .Where(box => !box.Name.Contains("Play"))
                .ToList();

            foreach (var image in TriggerImages)
            {
                image.Source = OffImage;
            }
            
            Select.Click += SelectOnClick;
        }



        private TrackStepViewModel ViewModel
        {
            get { return DataContext as TrackStepViewModel; }
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs arg)
        {
            var vm = ViewModel;

            vm.TracksChanged += TracksChanged;

            List<CheckBox> checkBoxes = Container.GetDescendantsOfType<CheckBox>()
               .Where(box => box.Name.Contains("Play"))
               .ToList();

            for (var i = 0;i < checkBoxes.Count;i++)
            {
                checkBoxes[i].IsChecked = ViewModel.GetAt(i);
            }
        }

        private void TracksChanged(object sender, EventArgs eventArgs)
        {
            _cancellationTokenSource.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            var step = (int) sender;
           
            if (token.IsCancellationRequested)
            {
                return;
            }
            
            _coreDispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                                    {
                                        foreach (var image in TriggerImages)
                                        {
                                            if (token.IsCancellationRequested)
                                            {
                                                return;
                                            }

                                            if (ReferenceEquals(image.Source, OnImage))
                                            {
                                                image.Source = OffImage;
                                            }
                                        }

                                        if (token.IsCancellationRequested)
                                        {
                                            return;
                                        }

                                        TriggerImages[step].Source = OnImage;
                                    });
        }

        private void FileExplorerOnDismiss(StorageTarget target, object file)
        {
            Select.IsChecked = false;

            var sample = file as IStorageFileEx;

            if (sample != null)
            {
                ViewModel.SelectSampleAsync(sample);
            }
        }

        private void Time_ValueChanged(object sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs)
        {
           // ViewModel.TimeStretch = Pitch.Value;
          //  PitchValue.Text = string.Format("Pitch: {0:0.0}", Pitch.Value);
        }

        private void SelectOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (Explorer == null)
            {
                Explorer = new FileExplorer
                           {
                               SelectionMode = FileExplorerUniversal.Control.Interop.SelectionMode.FileWithOpen,
                               StorageTarget = StorageTarget.InstalledLocation,
                               Width = ActualWidth,
                               Height = ActualHeight
                           };

                Explorer.OnDismiss += FileExplorerOnDismiss;
                Explorer.ExtensionRestrictions = ExtensionRestrictions.Custom;
                Explorer.Extensions = new[] {".wav", ".wave", ".seq"}.ToList();
            }

            Explorer.Show();
        }
    }
}