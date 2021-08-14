using LessonTimer.Services;
using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation.Metadata;
using Windows.Globalization;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LessonTimer
{
    public sealed partial class SettingsPage : Page
    {
        private bool countdownDescriptionNullLock;
        private bool alarmModeSoundLock;
        private bool settingsLoaded;

        private readonly MediaPlayer mediaPlayer;

        public SettingsPage()
        {
            this.InitializeComponent();
            this.LoadSettingsUI();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(MainTitleBar);

            ApplicationViewTitleBar titlebar = ApplicationView.GetForCurrentView().TitleBar;
            titlebar.ButtonBackgroundColor = Colors.Transparent;
            titlebar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titlebar.ButtonHoverBackgroundColor = (Color)Resources["SystemAccentColor"];
            titlebar.ButtonPressedBackgroundColor = (Color)Resources["SystemAccentColor"];
            titlebar.ButtonForegroundColor = (Application.Current.RequestedTheme == ApplicationTheme.Dark) ? Colors.White : Colors.Black;

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                Grid.Background = (Brush)Resources["SystemControlChromeMediumAcrylicWindowMediumBrush"];
                CloseButton.Style = (Style)Resources["ButtonRevealStyle"];
            }

            string flowDirectionSetting = ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
            this.FlowDirection = flowDirectionSetting switch
            {
                "LTR" => FlowDirection.LeftToRight,
                _ => FlowDirection.RightToLeft,
            };

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            VersionNumber.Text = String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

            mediaPlayer = new MediaPlayer();

            Application.Current.Suspending += new SuspendingEventHandler(App_Suspending);

            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CloseButton.Focus(FocusState.Programmatic);
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            MediaPlayer_Stop();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Application.Current.Suspending -= App_Suspending;
            mediaPlayer.Dispose();
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            MediaPlayer_Stop();
        }

        private void MediaPlayer_Stop()
        {
            mediaPlayer.Source = null;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void CountdownDescriptionTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Frame.GoBack();
            }
        }

        private void LectureLengthsTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Frame.GoBack();
            }
        }

        private void CountdownBaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            Settings.SetCountdownBase(item.Tag.ToString());

            EnableDisableLectureRoundComboBox();
        }

        private void CountdownDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }
            if (countdownDescriptionNullLock)
            {
                return;
            }

            Settings.SetCountdownDescription(CountdownDescriptionTextBox.Text);
        }

        private void CountdownDescriptionResetButton_Click(object sender, RoutedEventArgs e)
        {
            countdownDescriptionNullLock = true;
            Settings.SetCountdownDescription(null);

            ResourceLoader loader = new ResourceLoader();
            CountdownDescriptionTextBox.Text = loader.GetString("MinuteLecture");

            TriggerResetCountdownDescriptionNullLock();
        }

        private void LectureLengthsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            List<int> lectureLengths = new List<int>();

            int count = 0;
            string[] lengths = Regex.Split(LectureLengthsTextBox.Text, @"\s*(?:\uD805\uDC4D|\uD836\uDE87|[\u002C\u02BB\u060C\u2E32\u2E34\u2E41\u2E49\u3001\uFE10\uFE11\uFE50\uFE51\uFF0C\uFF64\u00B7\u055D\u07F8\u1363\u1802\u1808\uA4FE\uA60D\uA6F5\u02BD\u0312\u0313\u0314\u0315\u0326\u201A])\s*");
            foreach (string length in lengths)
            {
                if (count > 7)
                {
                    break;
                }

                try
                {
                    int value = NumberStrings.ParseLengthString24Hours(length);
                    lectureLengths.Add(value);
                    count++;
                }
                catch (Exception) { }
            }

            Settings.SetLectureLengths(lectureLengths);
        }

        private void LectureRoundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            Settings.SetLectureLengthRoundTo(Convert.ToInt32(item.Content));
        }

        private async void CalendarPrivacyButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-calendar"));
        }

        private void AcademicQuarterBeginToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            ToggleSwitch ts = sender as ToggleSwitch;
            Settings.SetAcademicQuarterBeginEnabled(ts.IsOn);
        }

        private void AcademicQuarterEndToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            ToggleSwitch ts = sender as ToggleSwitch;
            Settings.SetAcademicQuarterEndEnabled(ts.IsOn);
        }

        private void StartTimeCarryBackToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            ToggleSwitch ts = sender as ToggleSwitch;
            Settings.SetStartTimeCarryBackEnabled(ts.IsOn);
        }

        private void NotificationToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                Settings.SetNotificationsEnabled(true);
                NotificationSoundToggleSwitch.IsEnabled = true;

                if (NotificationSoundToggleSwitch.IsOn)
                {
                    NotificationSoundComboBox.IsEnabled = true;
                    NotificationAlarmModeToggleSwitch.IsEnabled = true;
                }

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleNotification();
                }
            }
            else
            {
                Settings.SetNotificationsEnabled(false);
                NotificationSoundToggleSwitch.IsEnabled = false;
                NotificationSoundComboBox.IsEnabled = false;
                NotificationAlarmModeToggleSwitch.IsEnabled = false;

                MainPage.CancelNotification();
                MediaPlayer_Stop();
            }
        }

        private void NotificationSoundToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                Settings.SetNotificationSoundEnabled(true);
                NotificationSoundComboBox.IsEnabled = true;
                NotificationAlarmModeToggleSwitch.IsEnabled = true;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleNotification();
                }
            }
            else
            {
                Settings.SetNotificationSoundEnabled(false);
                NotificationSoundComboBox.IsEnabled = false;
                NotificationAlarmModeToggleSwitch.IsEnabled = false;

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleNotification();
                }
                MediaPlayer_Stop();
            }
        }

        private void NotificationAlarmModeToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }
            if (alarmModeSoundLock)
            {
                alarmModeSoundLock = false;
                return;
            }

            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                Settings.SetNotificationAlarmModeEnabled(true);

                int fixedIndex = Settings.FixNotificationSoundForAlarmMode();
                if (fixedIndex != -1)
                {
                    MediaPlayer_Stop();
                    alarmModeSoundLock = true;
                    NotificationSoundComboBox.SelectedIndex = fixedIndex;
                }
            }
            else
            {
                Settings.SetNotificationAlarmModeEnabled(false);
            }

            if (CountdownLogic.Countdown.IsRunning)
            {
                MainPage.ScheduleNotification();
            }
        }

        private void NotificationSoundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }
            if (alarmModeSoundLock)
            {
                alarmModeSoundLock = false;
                return;
            }

            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            string soundUri = item.Tag.ToString();

            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(soundUri));
            mediaPlayer.Play();
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            Settings.SetNotificationSound(soundUri);

            if (Settings.FixAlarmModeForNotificationSound())
            {
                alarmModeSoundLock = true;
                NotificationAlarmModeToggleSwitch.IsOn = false;
            }

            if (CountdownLogic.Countdown.IsRunning)
            {
                MainPage.ScheduleNotification();
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            Settings.SetLanguageUI(item.Tag.ToString());

            ApplicationLanguages.PrimaryLanguageOverride = Settings.LanguageUI;

            if (Settings.NotificationsEnabled && CountdownLogic.Countdown.IsRunning)
            {
                MainPage.ScheduleNotification();
            }
        }

        private void ClockFormatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            RadioButton rb = sender as RadioButton;
            Settings.SetClockFormat(rb.Tag.ToString());
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            RadioButton rb = sender as RadioButton;
            Settings.SetTheme(rb.Tag.ToString());
        }

        private void LoadSettingsUI()
        {
            CountdownBaseComboBox.SelectedIndex = Settings.CountdownBase switch
            {
                "length" => 0,
                "time" => 1,
            };

            CountdownDescriptionTextBox.Text = Settings.GetCountdownDescription();

            EnableDisableLectureRoundComboBox();

            LectureLengthsTextBox.Text = String.Join(", ", Settings.LectureLengths.Select(
                    length => NumberStrings.NumberToCultureString(length)));

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
            StartTimeCarryBackToggleSwitch.IsOn = Settings.StartTimeCarryBackEnabled;

            if (Settings.NotificationsEnabled)
            {
                NotificationToggleSwitch.IsOn = true;
            }
            else
            {
                NotificationToggleSwitch.IsOn = false;
                NotificationSoundToggleSwitch.IsEnabled = false;
                NotificationSoundComboBox.IsEnabled = false;
                NotificationAlarmModeToggleSwitch.IsEnabled = false;
            }

            if (Settings.NotificationSoundEnabled)
            {
                NotificationSoundToggleSwitch.IsOn = true;
            }
            else
            {
                NotificationSoundToggleSwitch.IsOn = false;
                NotificationSoundComboBox.IsEnabled = false;
                NotificationAlarmModeToggleSwitch.IsEnabled = false;
            }

            NotificationSoundComboBox.SelectedIndex = Settings.AllowedNotificationSounds.IndexOf(Settings.NotificationSound);

            if (Settings.NotificationAlarmModeEnabled)
            {
                NotificationAlarmModeToggleSwitch.IsOn = true;
            }
            else
            {
                NotificationAlarmModeToggleSwitch.IsOn = false;
            }

            LanguageComboBox.SelectedIndex = Settings.LanguageUI switch
            {
                "ar" => 1,
                "zh-Hans" => 2,
                "en" => 3,
                "fr" => 4,
                "de" => 5,
                "it" => 6,
                "ja" => 7,
                "ko" => 8,
                "pt" => 9,
                "ru" => 10,
                "es" => 11,
                "tr" => 12,
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

            settingsLoaded = true;
        }

        private void EnableDisableLectureRoundComboBox()
        {
            LectureRoundComboBox.IsEnabled = Settings.CountdownBase switch
            {
                "length" => false,
                "time" => true,
            };
        }

        private async void TriggerResetCountdownDescriptionNullLock()
        {
            await Task.Delay(100);
            countdownDescriptionNullLock = false;
        }

        private async void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            StoreServicesFeedbackLauncher launcher = StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private async void RatingsButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9P4NPSWTX7LK"));
        }
    }
}
