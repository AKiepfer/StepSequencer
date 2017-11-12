using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ManagedAudioEngineUniversal.Core;
using UIUniversal.ViewModels;

namespace Sequencer_Universal
{
    public partial class MainPage : Page
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Add.Checked += AddOnChecked;

            CoverFlowControl.SelectionChanged += CoverFlowControlOnSelectedItemChanged;

            CoverFlowControl.UseTouchAnimationsForAllNavigation = true;
            
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private async void OnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
        {
            var frame = Window.Current.Content as Frame;

            if (frame != null && frame.Content == this)
            {
                backRequestedEventArgs.Handled = true;

                var yesCommand = new UICommand("Yes", command => Application.Current.Exit());
                var noCommand = new UICommand("No");

                var dialog = new MessageDialog("You want to stop making music?") { Commands = { yesCommand, noCommand } };

                await dialog.ShowAsync();
            }
        }

        private MainPageViewModel ViewModel
        {
            get { return DataContext as MainPageViewModel; }
            set { }
        }
        
        private async void AddOnChecked(object sender, RoutedEventArgs routedEventArgs)
        {
            await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                (() =>
                {
                    TrackStepViewModel track = ViewModel.AddTrack();

                    int index = ViewModel.TrackViewModels.IndexOf(track);

                    try
                    {
                        if (index != -1)
                        {
                            CoverFlowControl.SelectedIndex = index;
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        CoverFlowControl.SelectedIndex = 0;
                    }

                    Add.IsChecked = false;
                }));
        }

        private void CoverFlowControlOnSelectedItemChanged(object sender,
            SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (CoverFlowControl.SelectedItem == null)
            {
                return;
            }

            ViewModel.CurrentTrackViewModel = ViewModel.TrackViewModels[CoverFlowControl.SelectedIndex];
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var vm = e.Parameter as MainPageViewModel;

            if (vm != null && DataContext == null)
            {
                DataContext = vm;
            }

            base.OnNavigatedTo(e);
        }

        private async void ApplicationBarSaveButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Sequencer Project", new List<string> { ".seqProj" });
            
            var storageFile = await savePicker.PickSaveFileAsync();

            if (storageFile == null)
            {
                return;
            }

            var saveToFile = new StorageFileEx(storageFile);

            ViewModel.SaveProject(saveToFile);
        }

        private async void ApplicationBarLoadButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".seqProj");
            openPicker.CommitButtonText = "Load";

            var storageFile = await openPicker.PickSingleFileAsync();

            if (storageFile == null)
            {
                return;
            }

            var saveToFile = new StorageFileEx(storageFile);

            ViewModel.LoadProject(saveToFile);
        }

        private async void ApplicationBarRenderButton_OnClick(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.FileTypeChoices.Add("Track", new List<string> {".wav"});

            var storageFile = await savePicker.PickSaveFileAsync();

            if (storageFile == null)
            {
                return;
            }

            var saveToFile = new StorageFileEx(storageFile);

            ViewModel.RenderProject(saveToFile);
        }
    }
}