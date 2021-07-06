using LessonTimer.Services;
using System;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LessonTimer
{
    sealed partial class App : Application
    {
        public App()
        {
            Settings.LoadSettings();

            var notifications = ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications();
            if (notifications.Count != 0)
            {
                Notifications.UseToastNotification(notifications[0]);
            }

            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Settings.LanguageUI;

            this.RequestedTheme = Settings.Theme switch
            {
                "Light" => ApplicationTheme.Light,
                "Dark" => ApplicationTheme.Dark,
                _ => Current.RequestedTheme,
            };

            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content is null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args.Kind == ActivationKind.ToastNotification)
            {
                if (!(Window.Current.Content is Frame))
                {
                    Frame rootFrame = new Frame();
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    rootFrame.Navigate(typeof(MainPage));
                    Window.Current.Content = rootFrame;
                }
            }
            else if (args.Kind == ActivationKind.CommandLineLaunch)
            {
                if (!(Window.Current.Content is Frame rootFrame))
                {
                    rootFrame = new Frame();
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    Window.Current.Content = rootFrame;
                }

                rootFrame.Navigate(typeof(MainPage));
            }

            Window.Current.Activate();
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
