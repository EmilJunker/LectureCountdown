using LessonTimer.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
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
        private readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

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

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            VersionNumber.Text = String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
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

        private void CountdownBaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            Settings.CountdownBase = item.Tag.ToString();

            EnableDisableLectureRoundComboBox();

            localSettings.Values["countdownBase"] = Settings.CountdownBase;
        }

        private void LectureLengthsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.LectureLengths.Clear();

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
                        Settings.LectureLengths.Add(value);
                        localSettings.Values[String.Format("lectureLengths{0}", i)] = value;
                        i++;
                    }
                }
                catch (Exception) { }
            }

            if (i == 0)
            {
                Settings.LectureLengths = new List<int>(new int[] { 45, 60, 90 });
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
            Settings.LectureLengthRoundTo = Convert.ToInt32(item.Content);

            localSettings.Values["lectureLengthRoundTo"] = Settings.LectureLengthRoundTo;
        }

        private void AcademicQuarterBeginToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;

            Settings.AcademicQuarterBeginEnabled = ts.IsOn;

            localSettings.Values["academicQuarterBeginEnabled"] = Settings.AcademicQuarterBeginEnabled;
        }

        private void AcademicQuarterEndToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;

            Settings.AcademicQuarterEndEnabled = ts.IsOn;

            localSettings.Values["academicQuarterEndEnabled"] = Settings.AcademicQuarterEndEnabled;
        }

        private void NotificationToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                Settings.NotificationsEnabled = true;
                NotificationSoundToggleSwitch.IsEnabled = true;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleNotification();
                }
            }
            else
            {
                Settings.NotificationsEnabled = false;
                NotificationSoundToggleSwitch.IsEnabled = false;

                MainPage.CancelNotification();
            }

            localSettings.Values["notificationsEnabled"] = Settings.NotificationsEnabled;
        }

        private void NotificationSoundToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                Settings.NotificationSoundEnabled = true;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleNotification();
                }
            }
            else
            {
                Settings.NotificationSoundEnabled = false;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleNotification();
                }
            }

            localSettings.Values["notificationSoundEnabled"] = Settings.NotificationSoundEnabled;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            Settings.LanguageUI = item.Tag.ToString();

            ApplicationLanguages.PrimaryLanguageOverride = Settings.LanguageUI;

            localSettings.Values["languageUI"] = Settings.LanguageUI;
        }

        private void ClockFormatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            Settings.ClockFormat = rb.Tag.ToString() switch
            {
                "12" => "12HourClock",
                "24" => "24HourClock",
            };

            localSettings.Values["clockFormat"] = Settings.ClockFormat;
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            Settings.Theme = rb.Tag.ToString();

            localSettings.Values["theme"] = Settings.Theme;
        }

        private void LoadSettingsUI()
        {
            CountdownBaseComboBox.SelectedIndex = Settings.CountdownBase switch
            {
                "time" => 0,
                "length" => 1,
            };

            EnableDisableLectureRoundComboBox();

            LectureLengthsTextBox.Text += Settings.LectureLengths[0].ToString();
            for (int i = 1; i < Settings.LectureLengths.Count; i++)
            {
                LectureLengthsTextBox.Text += String.Format(", {0}", Settings.LectureLengths[i].ToString());
            }

            LectureRoundComboBox.SelectedIndex = Settings.LectureLengthRoundTo switch
            {
                30 => 0,
                15 => 1,
                10 => 2,
                5 => 3,
                1 => 4,
            };

            AcademicQuarterBeginToggleSwitch.IsOn = Settings.AcademicQuarterBeginEnabled;
            AcademicQuarterEndToggleSwitch.IsOn = Settings.AcademicQuarterEndEnabled;

            if (Settings.NotificationsEnabled)
            {
                NotificationToggleSwitch.IsOn = true;
                NotificationSoundToggleSwitch.IsEnabled = true;
            }
            else
            {
                NotificationToggleSwitch.IsOn = false;
                NotificationSoundToggleSwitch.IsEnabled = false;
            }

            NotificationSoundToggleSwitch.IsOn = Settings.NotificationSoundEnabled;

            LanguageComboBox.SelectedIndex = Settings.LanguageUI switch
            {
                "zh-Hans" => 1,
                "en-us" => 2,
                "fr-fr" => 3,
                "de-de" => 4,
                "ja-jp" => 5,
                "pt-pt" => 6,
                "ru-ru" => 7,
                "es-es" => 8,
                _ => 0,
            };

            switch (Settings.ClockFormat)
            {
                case "12HourClock":
                    ClockFormat12RadioButton.IsChecked = true;
                    break;
                case "24HourClock":
                    ClockFormat24RadioButton.IsChecked = true;
                    break;
            }

            switch (Settings.Theme)
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
            LectureRoundComboBox.IsEnabled = Settings.CountdownBase switch
            {
                "time" => true,
                "length" => false,
            };
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
