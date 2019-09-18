using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        public static String CountdownBase { get; private set; }
        public static List<int> LectureLengths { get; private set; }
        public static int LectureLengthRoundTo { get; private set; }
        public static bool NotificationsEnabled { get; private set; }
        public static bool NotificationSoundEnabled { get; private set; }
        public static String LanguageUI { get; private set; }
        public static String ClockFormat { get; private set; }
        public static String Theme { get; private set; }

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
                CloseButton.Style = (Style)Resources["ButtonRevealStyle"];
            }

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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private void CountdownBaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            CountdownBase = item.Tag.ToString();

            EnableDisableLectureRoundComboBox();

            localSettings.Values["countdownBase"] = CountdownBase;
        }

        private void LectureLengthsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LectureLengths.Clear();

            int i = 0;
            string[] lengths = Regex.Split(LectureLengthsTextBox.Text, @"\s*(?:\uD805\uDC4D|\uD836\uDE87|[\u002C\u02BB\u060C\u2E32\u2E34\u2E41\u2E49\u3001\uFE10\uFE11\uFE50\uFE51\uFF0C\uFF64\u00B7\u055D\u07F8\u1363\u1802\u1808\uA4FE\uA60D\uA6F5\u02BD\u0312\u0313\u0314\u0315\u0326\u201A])\s*");
            foreach (string length in lengths)
            {
                if (i > 7)
                {
                    break;
                }

                try
                {
                    var value = Convert.ToInt32(length);
                    if (0 < value && value < 1440)
                    {
                        LectureLengths.Add(value);
                        localSettings.Values[String.Format("lectureLengths{0}", i)] = value;
                        i++;
                    }
                }
                catch (Exception) { }
            }

            if (i == 0)
            {
                LectureLengths = new List<int>(new int[] { 45, 60, 90 });
                localSettings.Values["lectureLengths0"] = 45;
                localSettings.Values["lectureLengths1"] = 60;
                localSettings.Values["lectureLengths2"] = 90;
                i = 3;
            }

            while (i <= 7)
            {
                localSettings.Values[String.Format("lectureLengths{0}", i)] = null;
                i++;
            }
        }

        private void LectureRoundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            LectureLengthRoundTo = Convert.ToInt32(item.Content);

            localSettings.Values["lectureLengthRoundTo"] = LectureLengthRoundTo;
        }

        private void NotificationToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                NotificationsEnabled = true;
                NotificationSoundToggleSwitch.IsEnabled = true;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleToastNotification(NotificationSoundEnabled);
                }
            }
            else
            {
                NotificationsEnabled = false;
                NotificationSoundToggleSwitch.IsEnabled = false;

                MainPage.CancelToastNotification();
            }

            localSettings.Values["notificationsEnabled"] = NotificationsEnabled;
        }

        private void NotificationSoundToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                NotificationSoundEnabled = true;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleToastNotification(true);
                }
            }
            else
            {
                NotificationSoundEnabled = false;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleToastNotification(false);
                }
            }

            localSettings.Values["notificationSoundEnabled"] = NotificationSoundEnabled;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            LanguageUI = item.Tag.ToString();

            ApplicationLanguages.PrimaryLanguageOverride = LanguageUI;

            localSettings.Values["languageUI"] = LanguageUI;
        }

        private void ClockFormatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            switch (rb.Tag.ToString())
            {
                case "12":
                    ClockFormat = "12HourClock";
                    break;
                case "24":
                    ClockFormat = "24HourClock";
                    break;
            }

            localSettings.Values["clockFormat"] = ClockFormat;
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            Theme = rb.Tag.ToString();

            localSettings.Values["theme"] = Theme;
        }

        public static void LoadSettings(ApplicationDataContainer localSettings)
        {
            try
            {
                CountdownBase = (string)localSettings.Values["countdownBase"];

                if (CountdownBase == null)
                {
                    CountdownBase = "time";
                }
            }
            catch (NullReferenceException)
            {
                CountdownBase = "time";
            }

            try
            {
                LectureLengths = new List<int> { (int)localSettings.Values["lectureLengths0"] };
                try
                {
                    LectureLengths.Add((int)localSettings.Values["lectureLengths1"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths2"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths3"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths4"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths5"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths6"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths7"]);
                }
                catch (NullReferenceException) { }
            }
            catch (NullReferenceException)
            {
                LectureLengths = new List<int>(new int[] { 45, 60, 90 });
            }

            try
            {
                LectureLengthRoundTo = (int)localSettings.Values["lectureLengthRoundTo"];
            }
            catch (NullReferenceException)
            {
                LectureLengthRoundTo = 5;
            }

            try
            {
                NotificationsEnabled = (bool)localSettings.Values["notificationsEnabled"];
            }
            catch (NullReferenceException)
            {
                NotificationsEnabled = true;
            }

            try
            {
                NotificationSoundEnabled = (bool)localSettings.Values["notificationSoundEnabled"];
            }
            catch (NullReferenceException)
            {
                NotificationSoundEnabled = false;
            }

            try
            {
                LanguageUI = (string)localSettings.Values["languageUI"];

                if (LanguageUI == null)
                {
                    LanguageUI = String.Empty;
                }
            }
            catch (NullReferenceException)
            {
                LanguageUI = String.Empty;
            }

            try
            {
                ClockFormat = (string)localSettings.Values["clockFormat"];

                if (ClockFormat == null)
                {
                    ClockFormat = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code }).Clock;
                }
            }
            catch (NullReferenceException)
            {
                ClockFormat = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code }).Clock;
            }

            try
            {
                Theme = (string)localSettings.Values["theme"];

                if (Theme == null)
                {
                    Theme = String.Empty;
                }
            }
            catch (NullReferenceException)
            {
                Theme = String.Empty;
            }
        }

        private void LoadSettingsUI()
        {
            switch (CountdownBase)
            {
                case "time":
                    CountdownBaseComboBox.SelectedIndex = 0;
                    break;
                case "length":
                    CountdownBaseComboBox.SelectedIndex = 1;
                    break;
            }

            EnableDisableLectureRoundComboBox();

            LectureLengthsTextBox.Text += LectureLengths[0].ToString();
            for (int i = 1; i < LectureLengths.Count; i++)
            {
                LectureLengthsTextBox.Text += String.Format(", {0}", LectureLengths[i].ToString());
            }

            switch (LectureLengthRoundTo)
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

            if (NotificationsEnabled)
            {
                NotificationToggleSwitch.IsOn = true;
                NotificationSoundToggleSwitch.IsEnabled = true;
            }
            else
            {
                NotificationToggleSwitch.IsOn = false;
                NotificationSoundToggleSwitch.IsEnabled = false;
            }

            NotificationSoundToggleSwitch.IsOn = NotificationSoundEnabled;

            switch (LanguageUI)
            {
                case "zh-Hans":
                    LanguageComboBox.SelectedIndex = 1;
                    break;
                case "en-us":
                    LanguageComboBox.SelectedIndex = 2;
                    break;
                case "fr-fr":
                    LanguageComboBox.SelectedIndex = 3;
                    break;
                case "de-de":
                    LanguageComboBox.SelectedIndex = 4;
                    break;
                case "ja-jp":
                    LanguageComboBox.SelectedIndex = 5;
                    break;
                case "pt-pt":
                    LanguageComboBox.SelectedIndex = 6;
                    break;
                case "ru-ru":
                    LanguageComboBox.SelectedIndex = 7;
                    break;
                case "es-es":
                    LanguageComboBox.SelectedIndex = 8;
                    break;
                default:
                    LanguageComboBox.SelectedIndex = 0;
                    break;
            }

            switch (ClockFormat)
            {
                case "12HourClock":
                    ClockFormat12RadioButton.IsChecked = true;
                    break;
                case "24HourClock":
                    ClockFormat24RadioButton.IsChecked = true;
                    break;
            }

            switch (Theme)
            {
                case "Light":
                    ThemeLightRadioButton.IsChecked = true;
                    break;
                case "Dark":
                    ThemeDarkRadioButton.IsChecked = true;
                    break;
                default:
                    ThemeDefaultRadioButton.IsChecked = true;
                    break;
            }
        }

        private void EnableDisableLectureRoundComboBox()
        {
            switch (CountdownBase)
            {
                case "time":
                    LectureRoundComboBox.IsEnabled = true;
                    break;
                case "length":
                    LectureRoundComboBox.IsEnabled = false;
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
