// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using UIUniversal.ViewModels;

namespace Sequencer_Universal
{
    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
        private WinRTContainer _container;
    
        protected override void Configure()
        {
            var config = new TypeMappingConfiguration
            {
                DefaultSubNamespaceForViews = "UIUniversal.Views",
                DefaultSubNamespaceForViewModels = "UIUniversal.ViewModels"
            };
            ViewLocator.ConfigureTypeMappings(config);
            ViewModelLocator.ConfigureTypeMappings(config);

            _container = new WinRTContainer();

            _container.RegisterWinRTServices();
            _container.PerRequest<MainPageViewModel>();
            AddCustomConventions();

            var isStatusBarPresent = ApiInformation.IsTypePresent(typeof(StatusBar).ToString());

            if (isStatusBarPresent)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();

                // Hide the status bar
                statusBar.HideAsync();
            }
        }

        private static void AddCustomConventions()
        {
            //ellided  
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            _container.RegisterNavigationService(rootFrame);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                DisplayRootView(typeof (ExtendedSplashScreen));
            }
            else
            {
                DisplayRootView(typeof(ExtendedSplashScreen));
            }
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            List<Assembly> assemblies = base.SelectAssemblies()
                .ToList();
            assemblies.Add(typeof(TrackStepViewModel).GetTypeInfo()
                .Assembly);

            return assemblies;
        }
    }
}