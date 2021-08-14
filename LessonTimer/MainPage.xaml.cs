using CountdownLogic;
using LessonTimer.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LessonTimer
{
    public sealed partial class MainPage : Page
    {
        private static DateTime starttime;
        private static DateTime endtime;
        private static int length;
        private static string description;

        private DateTime? nextStarttime;
        private Description nextDescription;

        private bool suggestionAutoSetLock;
        private bool startButtonDisabledLock;

        private bool compactMode;

        public MainPage()
        {
            this.InitializeComponent();

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
                SuggestButton.Style = (Style)Resources["ButtonRevealStyle"];
                CalendarButton.Style = (Style)Resources["ButtonRevealStyle"];
                CancelButton.Style = (Style)Resources["ButtonRevealStyle"];
                StartButton.Style = (Style)Resources["ButtonRevealStyle"];
                SettingsButton.Style = (Style)Resources["ButtonRevealStyle"];
            }

            string flowDirectionSetting = ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
            this.FlowDirection = flowDirectionSetting switch
            {
                "LTR" => FlowDirection.LeftToRight,
                _ => FlowDirection.RightToLeft,
            };

            TimePicker.ClockIdentifier = Settings.ClockFormat;

            switch (Settings.CountdownBase)
            {
                case "length":
                    LengthPicker.Visibility = Visibility.Visible;
                    TimePicker.Visibility = Visibility.Collapsed;
                    break;
                case "time":
                    LengthPicker.Visibility = Visibility.Collapsed;
                    TimePicker.Visibility = Visibility.Visible;
                    break;
            }

            try
            {
                if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                {
                    CompactOverlayButton.Visibility = Visibility.Visible;
                }
                else
                {
                    CompactOverlayButton.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                CompactOverlayButton.Visibility = Visibility.Collapsed;
            }

            nextStarttime = null;
            nextDescription = new Description();

            Size size = new Size(400, 300);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(size);

            Countdown.tick.Ticked += new TickEventHandler(UpdateCountdown);

            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Countdown.TimerSetup(starttime, endtime);

                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                startButtonDisabledLock = true;

                CancelButton.Focus(FocusState.Programmatic);

                string message = Description.GetDescriptionString(description, length);
                DisplayMessage(message, false);

                if (Settings.NotificationsEnabled)
                {
                    ScheduleNotification();
                }

                switch (Settings.CountdownBase)
                {
                    case "length":
                        LengthPicker.Text = length.ToString();
                        break;
                    case "time":
                        TimePicker.Time = new TimeSpan(endtime.Hour, endtime.Minute, 0);
                        break;
                }
            }
            catch (ArgumentException)
            {
                TimeSuggestions.SuggestionsIterator = 0;
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
                suggestionAutoSetLock = true;

                StartButton.Focus(FocusState.Programmatic);

                int suggestionLength = 0;
                switch (Settings.CountdownBase)
                {
                    case "length":
                        (LengthPicker.Text, suggestionLength) = TimeSuggestions.GetLengthSuggestion();
                        TriggerResetSuggestionAutoSetLock();
                        break;
                    case "time":
                        (TimePicker.Time, suggestionLength) = TimeSuggestions.GetEndTimeSuggestion();
                        TriggerResetSuggestionAutoSetLock();
                        break;
                }
                nextStarttime = null;
                nextDescription.Set(suggestionLength);
            }
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                e.Handled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            if (e.NavigationMode == NavigationMode.New)
            {
                RestoreSession();

                if (e.Parameter != null && e.Parameter is ParsedOptions options)
                {
                    ApplyCommandLineOptions(options);
                }
            }
        }

        private void ApplyCommandLineOptions(ParsedOptions options)
        {
            if (options.cancelCountdown)
            {
                CancelCountdown();
            }
            else if (options.starttime != null && options.endtime != null && options.length != null)
            {
                starttime = options.starttime.Value;
                endtime = options.endtime.Value;
                length = options.length.Value;
                description = options.description;
            }
            else if (options.description != null)
            {
                description = options.description;
            }

            SaveSession();

            if (options.notificationMode != null)
            {
                if (options.notificationMode == "none")
                {
                    Settings.SetNotificationsEnabled(false);
                }
                else if (options.notificationMode == "silent")
                {
                    Settings.SetNotificationsEnabled(true);
                    Settings.SetNotificationSoundEnabled(false);
                }
                else if (options.notificationMode == "sound")
                {
                    Settings.SetNotificationsEnabled(true);
                    Settings.SetNotificationSoundEnabled(true);
                    Settings.SetNotificationAlarmModeEnabled(false);
                }
                else if (options.notificationMode == "alarm")
                {
                    Settings.SetNotificationsEnabled(true);
                    Settings.SetNotificationSoundEnabled(true);
                    Settings.SetNotificationAlarmModeEnabled(true);
                    Settings.FixNotificationSoundForAlarmMode();
                }
            }

            if (options.notificationSound != null)
            {
                Settings.SetNotificationSound(options.notificationSound);
                Settings.FixAlarmModeForNotificationSound();
            }

            if (options.launchInCompactMode)
            {
                CompactOverlayOn();
            }
            else
            {
                CompactOverlayOff();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void LengthPicker_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                StartCountdown();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartCountdown();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelCountdown();
        }

        private void CancelCountdown()
        {
            if (Countdown.IsRunning)
            {
                Countdown.EndCountdown();

                endtime = DateTime.Now.AddSeconds(-1);
                SaveSession();

                CancelNotification();

                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
            }
        }

        private void StartCountdown()
        {
            DateTime _starttime;
            if (nextStarttime != null && nextStarttime < DateTime.Now && nextStarttime > DateTime.Now.AddDays(-1))
            {
                _starttime = (DateTime)nextStarttime;
            }
            else
            {
                _starttime = Countdown.DateTimeNow();
            }

            bool success = false;

            switch (Settings.CountdownBase)
            {
                case "length":
                    try
                    {
                        string lengthString = LengthPicker.Text;

                        int _length = NumberStrings.ParseLengthString24Hours(lengthString);

                        endtime = Countdown.TimerSetup(_starttime, _length);
                        length = _length;
                        success = true;
                    }
                    catch (Exception) { }
                    break;

                case "time":
                    try
                    {
                        int hour = TimePicker.Time.Hours;
                        int min = TimePicker.Time.Minutes;

                        DateTime _endtime = Countdown.DateTimeTodayOrTomorrow(hour, min, 0);

                        int countdownLength = Countdown.TimerSetup(_starttime, _endtime);
                        if (nextDescription.CountdownLength == 0)
                        {
                            length = countdownLength;
                        }
                        else
                        {
                            length = nextDescription.CountdownLength;
                        }
                        endtime = _endtime;
                        success = true;
                    }
                    catch (Exception) { }
                    break;
            }

            if (success)
            {
                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                CancelButton.Focus(FocusState.Programmatic);

                starttime = _starttime;
                nextStarttime = null;

                description = nextDescription.CountdownDescription;
                nextDescription.Reset();

                string message = Description.GetDescriptionString(description, length);
                DisplayMessage(message, false);

                if (Settings.NotificationsEnabled)
                {
                    ScheduleNotification();
                }

                SaveSession();
            }
            else
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                DisplayMessage(loader.GetString("ErrorInvalidInput"), true);
            }
        }

        private async void UpdateCountdown(object source, TickEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CountdownLabel.Text = e.Countdown;
                CountdownProgressBar.Value = e.Progress;
                CountdownProgressText.Text = e.Timeprogress;
                CountdownProgressPercentage.Text = e.Percentprogress;

                if (!Countdown.IsRunning)
                {
                    StartButton.IsEnabled = true;
                    CancelButton.IsEnabled = false;
                    InfoTextBlock.Text = String.Empty;
                    ToolTipService.SetToolTip(InfoTextBlock, null);
                }
            });
        }

        public static void ScheduleNotification()
        {
            Notifications.ScheduleToastNotification(Settings.NotificationSoundEnabled, Settings.NotificationAlarmModeEnabled, Settings.NotificationSound, starttime, endtime);
        }

        public static void CancelNotification()
        {
            Notifications.CancelToastNotification();
        }

        private void LengthPicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (suggestionAutoSetLock)
            {
                suggestionAutoSetLock = false;
            }
            else if (startButtonDisabledLock)
            {
                startButtonDisabledLock = false;
            }
            else
            {
                nextStarttime = null;
                nextDescription.Reset();
                StartButton.IsEnabled = true;
            }
        }

        private void TimePicker_Changed(object sender, TimePickerValueChangedEventArgs e)
        {
            if (suggestionAutoSetLock)
            {
                suggestionAutoSetLock = false;
            }
            else if (startButtonDisabledLock)
            {
                startButtonDisabledLock = false;
            }
            else if (e.NewTime != e.OldTime)
            {
                nextStarttime = null;
                nextDescription.Reset();
                StartButton.IsEnabled = true;
            }
        }

        private void SuggestButton_Click(object sender, RoutedEventArgs e)
        {
            nextStarttime = null;

            int suggestionLength = 0;
            suggestionAutoSetLock = true;

            switch (Settings.CountdownBase)
            {
                case "length":
                    (LengthPicker.Text, suggestionLength) = TimeSuggestions.GetLengthSuggestion();
                    TriggerResetSuggestionAutoSetLock();
                    break;
                case "time":
                    (TimePicker.Time, suggestionLength) = TimeSuggestions.GetEndTimeSuggestion();
                    TriggerResetSuggestionAutoSetLock();
                    break;
            }

            nextDescription.Set(suggestionLength);

            if (Countdown.IsRunning)
            {
                StartButton.IsEnabled = true;
            }
            else
            {
                string message = Description.GetDescriptionString(suggestionLength);
                DisplayMessage(message, true);
            }
        }

        private async void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            DateTimeOffset dateToShow = DateTime.Now;
            TimeSpan duration = TimeSpan.FromHours(24);

            string appointmentDescription = null;
            suggestionAutoSetLock = true;

            try
            {
                var allAppointments = await appointmentStore.FindAppointmentsAsync(dateToShow, duration);

                switch (Settings.CountdownBase)
                {
                    case "length":
                        var lengthSuggestion = TimeSuggestions.GetLengthSuggestion(allAppointments);
                        nextStarttime = lengthSuggestion.starttime;
                        if (lengthSuggestion.description != null)
                        {
                            LengthPicker.Text = lengthSuggestion.lengthString;
                            appointmentDescription = lengthSuggestion.description;
                            TriggerResetSuggestionAutoSetLock();
                        }
                        break;
                    case "time":
                        var endTimeSuggestion = TimeSuggestions.GetEndTimeSuggestion(allAppointments);
                        nextStarttime = endTimeSuggestion.starttime;
                        if (endTimeSuggestion.description != null)
                        {
                            TimePicker.Time = endTimeSuggestion.endtimePick;
                            appointmentDescription = endTimeSuggestion.description;
                            TriggerResetSuggestionAutoSetLock();
                        }
                        break;
                }

                if (appointmentDescription != null)
                {
                    nextDescription.Set(appointmentDescription);

                    if (Countdown.IsRunning)
                    {
                        StartButton.IsEnabled = true;
                    }
                    else
                    {
                        DisplayMessage(appointmentDescription, true);
                    }
                }
                else
                {
                    if (!Countdown.IsRunning)
                    {
                        var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                        DisplayMessage(loader.GetString("ErrorNoEventsInCalendar"), true);
                    }
                }
            }
            catch (NullReferenceException)
            {
                if (!Countdown.IsRunning)
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    DisplayMessage(loader.GetString("ErrorNoCalendarAccessPermission"), true);
                }
            }
        }

        private void DisplayMessage(string message, bool fadeout)
        {
            ResetFadeStoryboard.Begin();
            InfoTextBlock.Text = message;

            if (fadeout)
            {
                FadeOutStoryboard.Begin();
                ToolTipService.SetToolTip(InfoTextBlock, null);
            }
            else if (message.Length == 0)
            {
                ToolTipService.SetToolTip(InfoTextBlock, null);
            }
            else
            {
                ToolTipService.SetToolTip(InfoTextBlock, message);
            }
        }

        private void CompactOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (compactMode)
            {
                CompactOverlayOff();
            }
            else
            {
                CompactOverlayOn();
            }
        }

        private async void CompactOverlayOn()
        {
            ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            compactOptions.CustomSize = new Size(320, 160);
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);

            ControlPanel.Visibility = Visibility.Collapsed;
            compactMode = true;
        }

        private async void CompactOverlayOff()
        {
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

            ControlPanel.Visibility = Visibility.Visible;
            compactMode = false;
        }

        private async void TriggerResetSuggestionAutoSetLock()
        {
            await Task.Delay(100);
            suggestionAutoSetLock = false;
        }

        private void SaveSession()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values["starttime"] = starttime.Ticks;
            localSettings.Values["endtime"] = endtime.Ticks;
            localSettings.Values["length"] = length;
            localSettings.Values["description"] = description;
        }

        private void RestoreSession()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            try
            {
                starttime = new DateTime((long)localSettings.Values["starttime"]);
                endtime = new DateTime((long)localSettings.Values["endtime"]);
                length = (int)localSettings.Values["length"];
                description = (string)localSettings.Values["description"];
            }
            catch (NullReferenceException) { }
            catch (ArgumentOutOfRangeException) { }
            catch (InvalidCastException) { }
        }
    }
}
