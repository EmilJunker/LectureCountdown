using LessonTimer.Services;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
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

            RestoreSession();

            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Settings.LanguageUI;

            this.RequestedTheme = Settings.Theme switch
            {
                "Light" => ApplicationTheme.Light,
                "Dark" => ApplicationTheme.Dark,
                _ => Current.RequestedTheme,
            };

            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        private void RestoreSession()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            try
            {
                MainPage.starttime = new DateTime((long)localSettings.Values["starttime"]);
                MainPage.endtime = new DateTime((long)localSettings.Values["endtime"]);
                MainPage.length = (int)localSettings.Values["length"];
                MainPage.description = (string)localSettings.Values["description"];
            }
            catch (NullReferenceException) { }
            catch (ArgumentOutOfRangeException) { }
            catch (InvalidCastException) { }
        }

        private void SaveSession()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values["starttime"] = MainPage.starttime.Ticks;
            localSettings.Values["endtime"] = MainPage.endtime.Ticks;
            localSettings.Values["length"] = MainPage.length;
            localSettings.Values["description"] = MainPage.description;
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
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args.Kind == ActivationKind.ToastNotification)
            {
                if (!(Window.Current.Content is Frame))
                {
                    Frame rootFrame = new Frame();
                    rootFrame.Navigate(typeof(MainPage));
                    Window.Current.Content = rootFrame;
                }

                RestoreSession();
            }

            Window.Current.Activate();
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            //var deferral = e.SuspendingOperation.GetDeferral();
            SaveSession();
            //deferral.Complete();
        }
    }
}
