using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using UIUniversal;
using UIUniversal.ViewModels;

namespace Sequencer_Universal
{
    public partial class ExtendedSplashScreen : Page
    {
        private CancellationTokenSource stopAnimation = new CancellationTokenSource();

        public ExtendedSplashScreen()
        {
            InitializeComponent();

            // HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
            ShowSplash();
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;

            if (frame != null && frame.Content == this)
            {
                e.Handled = true;
            }
        }

        async void ShowSplash()
        {
            var token = stopAnimation.Token;

            var triggerBoxes = LayoutRoot.GetDescendantsOfType<CheckBox>().Where(box => box.Name.Contains("Led")).ToList();

            var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            Task.Run(() =>
            {
                CheckBox previous = null;

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    foreach (var triggerBox in triggerBoxes)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }


                        Execute.OnUIThread(() =>
                                                {
                                                    if (previous != null)
                                                    {
                                                        previous.IsChecked = false;
                                                    }

                                                    triggerBox.IsChecked = true;

                                                    previous = triggerBox;
                                                });

                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        Task.Delay(200).Wait();
                    }
                }

            }, token);

            var mainVm = new MainPageViewModel();

            await Task.Delay(TimeSpan.FromSeconds(2));

            stopAnimation.Cancel();

            var rootFrame = new Frame();
            
            // Navigate to mainpage 
            rootFrame.Navigate(typeof(MainPage), mainVm);

            // Place the frame in the current Window 
            Window.Current.Content = rootFrame; 
        }


    }
}