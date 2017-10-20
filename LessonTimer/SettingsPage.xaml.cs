using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.Globalization;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LessonTimer
{
    public sealed partial class SettingsPage : Page
    {
        private static List<int> lectureLengths;
        public static List<int> LectureLengths
        {
            get { return lectureLengths; }
        }
        private static int lectureLengthRoundTo;
        public static int LectureLengthRoundTo
        {
            get { return lectureLengthRoundTo; }
        }
        private static bool notificationsEnabled;
        public static bool NotificationsEnabled
        {
            get { return notificationsEnabled; }
        }
        private static bool notificationSoundEnabled;
        public static bool NotificationSoundEnabled
        {
            get { return notificationSoundEnabled; }
        }
        private static String languageUI;
        public static String LanguageUI
        {
            get { return languageUI; }
        }
        private static String clockFormat;
        public static String ClockFormat
        {
            get { return clockFormat; }
        }
        private static String theme;
        public static String Theme
        {
            get { return theme; }
        }

        public SettingsPage()
        {
            this.InitializeComponent();
            this.LoadSettingsUI();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(MainTitleBar);

            ApplicationViewTitleBar titlebar = ApplicationView.GetForCurrentView().TitleBar;
            titlebar.ButtonBackgroundColor = Colors.Transparent;
            titlebar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titlebar.ButtonHoverBackgroundColor = (Color)this.Resources["SystemAccentColor"];
            titlebar.ButtonPressedBackgroundColor = (Color)this.Resources["SystemAccentColor"];
            titlebar.ButtonForegroundColor = (Application.Current.RequestedTheme == ApplicationTheme.Dark) ? Colors.White : Colors.Black;

            if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                Grid.Background = (Windows.UI.Xaml.Media.Brush)Resources["SystemControlChromeMediumAcrylicWindowMediumBrush"];
                CloseButton.Style= (Style)Resources["ButtonRevealStyle"];
            }

            SystemNavigationManager.GetForCurrentView().BackRequested += SettingsPage_BackRequested;

            this.Loaded += Page_Loaded;
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CloseButton.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void SettingsPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            e.Handled = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private void LectureLengthsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            lectureLengths.Clear();
            string[] lengthsList = LectureLengthsTextBox.Text.Split(',');
            for (int i = 0; i < 8; i++)
            {
                try
                {
                    var value = Convert.ToInt32(lengthsList[i].Trim());
                    lectureLengths.Add(value);
                    localSettings.Values[String.Format("lectureLengths{0}", i)] = value;
                }
                catch (Exception)
                {
                    if (i == 0)
                    {
                        lectureLengths.Add(90);
                        localSettings.Values["lectureLengths0"] = 90;
                    }
                    else
                    {
                        localSettings.Values[String.Format("lectureLengths{0}", i)] = null;
                    }
                }
            }
        }

        private void LectureRoundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            lectureLengthRoundTo = Convert.ToInt32(item.Content);

            localSettings.Values["lectureLengthRoundTo"] = lectureLengthRoundTo;
        }

        private void NotificationToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                notificationsEnabled = true;
                NotificationSoundToggleSwitch.IsEnabled = true;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleToastNotification(notificationSoundEnabled);
                }
            }
            else
            {
                notificationsEnabled = false;
                NotificationSoundToggleSwitch.IsEnabled = false;

                MainPage.CancelToastNotification();
            }

            localSettings.Values["notificationsEnabled"] = notificationsEnabled;
        }

        private void NotificationSoundToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                notificationSoundEnabled = true;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleToastNotification(true);
                }
            }
            else
            {
                notificationSoundEnabled = false;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleToastNotification(false);
                }
            }

            localSettings.Values["notificationSoundEnabled"] = notificationSoundEnabled;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            languageUI = item.Tag.ToString();

            ApplicationLanguages.PrimaryLanguageOverride = languageUI;

            localSettings.Values["languageUI"] = languageUI;
        }

        private void ClockFormatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            switch (rb.Tag.ToString())
            {
                case "12":
                    clockFormat = "12HourClock";
                    break;
                case "24":
                    clockFormat = "24HourClock";
                    break;
            }

            localSettings.Values["clockFormat"] = clockFormat;
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            switch (rb.Tag.ToString())
            {
                case "Light":
                    theme = "Light";
                    break;
                case "Dark":
                    theme = "Dark";
                    break;
            }

            localSettings.Values["theme"] = theme;
        }

        public static void LoadSettings(ApplicationDataContainer localSettings)
        {
            try
            {
                lectureLengths = new List<int> { (int)localSettings.Values["lectureLengths0"] };
                try
                {
                    lectureLengths.Add((int)localSettings.Values["lectureLengths1"]);
                    lectureLengths.Add((int)localSettings.Values["lectureLengths2"]);
                    lectureLengths.Add((int)localSettings.Values["lectureLengths3"]);
                    lectureLengths.Add((int)localSettings.Values["lectureLengths4"]);
                    lectureLengths.Add((int)localSettings.Values["lectureLengths5"]);
                    lectureLengths.Add((int)localSettings.Values["lectureLengths6"]);
                    lectureLengths.Add((int)localSettings.Values["lectureLengths7"]);
                }
                catch (NullReferenceException) { }
            }
            catch (NullReferenceException)
            {
                lectureLengths = new List<int>(new int[] { 60, 90 });
            }

            try
            {
                lectureLengthRoundTo = (int)localSettings.Values["lectureLengthRoundTo"];
            }
            catch (NullReferenceException)
            {
                lectureLengthRoundTo = 15;
            }

            try
            {
                notificationsEnabled = (bool)localSettings.Values["notificationsEnabled"];
            }
            catch (NullReferenceException)
            {
                notificationsEnabled = true;
            }

            try
            {
                notificationSoundEnabled = (bool)localSettings.Values["notificationSoundEnabled"];
            }
            catch (NullReferenceException)
            {
                notificationSoundEnabled = false;
            }

            try
            {
                languageUI = (String)localSettings.Values["languageUI"];

                if (languageUI == null)
                {
                    languageUI = "en";
                }
            }
            catch (NullReferenceException)
            {
                languageUI = "en";
            }

            try
            {
                clockFormat = (String)localSettings.Values["clockFormat"];

                if (clockFormat == null)
                {
                    clockFormat = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code }).Clock;
                }
            }
            catch (NullReferenceException)
            {
                clockFormat = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code }).Clock;
            }

            try
            {
                theme = (String)localSettings.Values["theme"];

                if (theme == null)
                {
                    theme = "Dark";
                }
            }
            catch (NullReferenceException)
            {
                theme = "Dark";
            }
        }

        private void LoadSettingsUI()
        {
            LectureLengthsTextBox.Text += lectureLengths[0].ToString();
            for (int i = 1; i < lectureLengths.Count; i++)
            {
                LectureLengthsTextBox.Text += String.Format(", {0}", lectureLengths[i].ToString());
            }

            switch (lectureLengthRoundTo)
            {
                case 30:
                    LectureRoundComboBox.SelectedIndex = 0;
                    break;
                case 15:
                    LectureRoundComboBox.SelectedIndex = 1;
                    break;
                case 10:
                    LectureRoundComboBox.SelectedIndex = 2;
                    break;
                case 5:
                    LectureRoundComboBox.SelectedIndex = 3;
                    break;
                case 1:
                    LectureRoundComboBox.SelectedIndex = 4;
                    break;
                default:
                    LectureRoundComboBox.SelectedIndex = 1;
                    break;
            }

            if (notificationsEnabled)
            {
                NotificationToggleSwitch.IsOn = true;
                NotificationSoundToggleSwitch.IsEnabled = true;
            }
            else
            {
                NotificationToggleSwitch.IsOn = false;
                NotificationSoundToggleSwitch.IsEnabled = false;
            }

            NotificationSoundToggleSwitch.IsOn = notificationSoundEnabled;

            switch (languageUI)
            {
                case "en-us":
                    LanguageComboBox.SelectedIndex = 0;
                    break;
                case "de-de":
                    LanguageComboBox.SelectedIndex = 1;
                    break;
                default:
                    LanguageComboBox.SelectedIndex = 0;
                    break;
            }

            switch (clockFormat)
            {
                case "12HourClock":
                    ClockFormat12RadioButton.IsChecked = true;
                    break;
                case "24HourClock":
                    ClockFormat24RadioButton.IsChecked = true;
                    break;
            }

            switch (theme)
            {
                case "Light":
                    ThemeLightRadioButton.IsChecked = true;
                    break;
                case "Dark":
                    ThemeDarkRadioButton.IsChecked = true;
                    break;
            }
        }

        private async void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private async void RatingsButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9P4NPSWTX7LK"));
        }
    }
}
