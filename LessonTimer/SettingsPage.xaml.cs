using LessonTimer.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Globalization;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.System;
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
        private bool countdownDescriptionNullLock;
        private bool alarmModeSoundLock;
        private bool settingsLoaded;

        private readonly List<int> alarmForbiddenSounds;
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

            alarmForbiddenSounds = new List<int> { 0, 1, 2, 3 };
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

        private void CountdownDescriptionTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Frame.GoBack();
            }
        }

        private void LectureLengthsTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                Frame.GoBack();
            }
        }

        private void CountdownBaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            Settings.SetCountdownBase(item.Tag.ToString());

            EnableDisableLectureRoundComboBox();
        }

        private void CountdownDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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

            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            CountdownDescriptionTextBox.Text = loader.GetString("MinuteLecture");

            TriggerResetCountdownDescriptionNullLock();
        }

        private void LectureLengthsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
                    int value = Convert.ToInt32(length);
                    if (0 < value && value < 1440)
                    {
                        lectureLengths.Add(value);
                        count++;
                    }
                }
                catch (Exception) { }
            }

            Settings.SetLectureLengths(lectureLengths);
        }

        private void LectureRoundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            ToggleSwitch ts = sender as ToggleSwitch;
            Settings.SetAcademicQuarterBeginEnabled(ts.IsOn);
        }

        private void AcademicQuarterEndToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;
            Settings.SetAcademicQuarterEndEnabled(ts.IsOn);
        }

        private void StartTimeCarryBackToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;
            Settings.SetStartTimeCarryBackEnabled(ts.IsOn);
        }

        private void NotificationToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
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

            ToggleSwitch ts = sender as ToggleSwitch;

            if (ts.IsOn)
            {
                Settings.SetNotificationAlarmModeEnabled(true);

                if (alarmForbiddenSounds.IndexOf(NotificationSoundComboBox.SelectedIndex) != -1)
                {
                    MediaPlayer_Stop();
                    alarmModeSoundLock = true;
                    NotificationSoundComboBox.SelectedIndex = alarmForbiddenSounds.Count;
                }
                else if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleNotification();
                }
            }
            else
            {
                Settings.SetNotificationAlarmModeEnabled(false);

                if (CountdownLogic.Countdown.IsRunning)
                {
                    MainPage.ScheduleNotification();
                }
            }
        }

        private void NotificationSoundComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!settingsLoaded)
            {
                return;
            }

            ComboBox cb = sender as ComboBox;
            ComboBoxItem item = cb.SelectedItem as ComboBoxItem;
            string soundUri = item.Tag.ToString();

            if (alarmModeSoundLock)
            {
                alarmModeSoundLock = false;
            }
            else
            {
                mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(soundUri));
                mediaPlayer.Play();
                mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            }

            Settings.SetNotificationSound(soundUri);

            if (NotificationAlarmModeToggleSwitch.IsOn && alarmForbiddenSounds.IndexOf(cb.SelectedIndex) != -1)
            {
                NotificationAlarmModeToggleSwitch.IsOn = false;
            }
            else if (CountdownLogic.Countdown.IsRunning)
            {
                MainPage.ScheduleNotification();
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            RadioButton rb = sender as RadioButton;
            Settings.SetClockFormat(rb.Tag.ToString());
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
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

            LectureLengthsTextBox.Text = String.Join(", ", Settings.LectureLengths);

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

            NotificationSoundComboBox.SelectedIndex = Settings.NotificationSound switch
            {
                "ms-winsoundevent:Notification.Default" => 0,
                "ms-winsoundevent:Notification.IM" => 1,
                "ms-winsoundevent:Notification.Mail" => 2,
                "ms-winsoundevent:Notification.Reminder" => 3,
                "ms-winsoundevent:Notification.Looping.Alarm" => 4,
                "ms-winsoundevent:Notification.Looping.Alarm2" => 5,
                "ms-winsoundevent:Notification.Looping.Alarm3" => 6,
                "ms-winsoundevent:Notification.Looping.Alarm4" => 7,
                "ms-winsoundevent:Notification.Looping.Alarm5" => 8,
                "ms-winsoundevent:Notification.Looping.Alarm6" => 9,
                "ms-winsoundevent:Notification.Looping.Alarm7" => 10,
                "ms-winsoundevent:Notification.Looping.Alarm8" => 11,
                "ms-winsoundevent:Notification.Looping.Alarm9" => 12,
                "ms-winsoundevent:Notification.Looping.Alarm10" => 13,
            };

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
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private async void RatingsButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9P4NPSWTX7LK"));
        }
    }
}
