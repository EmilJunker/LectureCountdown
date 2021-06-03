using CountdownLogic;
using LessonTimer.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
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
        bool compactMode;

        public static bool suggestionAutoSetLock;
        public static bool startButtonDisabledLock;

        public static DateTime starttime;
        public static DateTime endtime;
        public static int length;
        public static string description;

        public static Description nextDescription;

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

            compactMode = false;
            nextDescription = new Description();

            Size size = new Size(400, 300);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(size);

            Countdown.tick.Ticked += new TickEventHandler(UpdateCountdown);

            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (endtime > DateTime.Now)
            {
                Countdown.TimerSetup(starttime, endtime);

                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                startButtonDisabledLock = true;

                CancelButton.Focus(FocusState.Programmatic);

                string message = Description.GetDescriptionString(description, length);
                DisplayMessage(message, false);

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
            else
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
                        TriggerResetSuggestionAutoSetLog();
                        break;
                    case "time":
                        (TimePicker.Time, suggestionLength) = TimeSuggestions.GetEndTimeSuggestion();
                        TriggerResetSuggestionAutoSetLog();
                        break;
                }
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
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
            Countdown.CountdownIsOver();

            endtime = DateTime.Now;

            CancelNotification();

            StartButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }

        private void StartCountdown()
        {
            starttime = Countdown.DateTimeNow();

            bool success = false;

            switch (Settings.CountdownBase)
            {
                case "length":
                    try
                    {
                        int inputLength = Convert.ToInt32(LengthPicker.Text);
                        if (0 < inputLength && inputLength < 1440)
                        {
                            length = inputLength;
                            endtime = Countdown.TimerSetup(starttime, length);
                            success = true;
                        }
                    }
                    catch (Exception) { }
                    finally
                    {
                        if (!success)
                        {
                            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                            DisplayMessage(loader.GetString("ErrorInvalidLength"), true);
                        }
                    }
                    break;

                case "time":
                    int hour = TimePicker.Time.Hours;
                    int min = TimePicker.Time.Minutes;

                    endtime = Countdown.DateTimeTodayOrTomorrow(hour, min, 0);

                    int countdownLength = Countdown.TimerSetup(starttime, endtime);
                    if (nextDescription.CountdownLength == 0)
                    {
                        length = countdownLength;
                    }
                    else
                    {
                        length = nextDescription.CountdownLength;
                    }
                    success = true;
                    break;
            }

            if (success)
            {
                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                CancelButton.Focus(FocusState.Programmatic);

                description = nextDescription.CountdownDescription;
                nextDescription.Reset();

                string message = Description.GetDescriptionString(description, length);
                DisplayMessage(message, false);

                if (Settings.NotificationsEnabled)
                {
                    ScheduleNotification();
                }
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
                    endtime = DateTime.Now;
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
                nextDescription.Reset();
                StartButton.IsEnabled = true;
            }
        }

        private void SuggestButton_Click(object sender, RoutedEventArgs e)
        {
            int suggestionLength = 0;
            suggestionAutoSetLock = true;

            switch (Settings.CountdownBase)
            {
                case "length":
                    (LengthPicker.Text, suggestionLength) = TimeSuggestions.GetLengthSuggestion();
                    TriggerResetSuggestionAutoSetLog();
                    break;
                case "time":
                    (TimePicker.Time, suggestionLength) = TimeSuggestions.GetEndTimeSuggestion();
                    TriggerResetSuggestionAutoSetLog();
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
                        if (lengthSuggestion.description != null)
                        {
                            LengthPicker.Text = lengthSuggestion.lengthString;
                            appointmentDescription = lengthSuggestion.description;
                            TriggerResetSuggestionAutoSetLog();
                        }
                        break;
                    case "time":
                        var endTimeSuggestion = TimeSuggestions.GetEndTimeSuggestion(allAppointments);
                        if (endTimeSuggestion.description != null)
                        {
                            TimePicker.Time = endTimeSuggestion.endtimePick;
                            appointmentDescription = endTimeSuggestion.description;
                            TriggerResetSuggestionAutoSetLog();
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
            _ = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);

            ControlPanel.Visibility = Visibility.Collapsed;
            compactMode = true;
        }

        private async void CompactOverlayOff()
        {
            _ = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

            ControlPanel.Visibility = Visibility.Visible;
            compactMode = false;
        }

        private async void TriggerResetSuggestionAutoSetLog()
        {
            await Task.Delay(100);
            suggestionAutoSetLock = false;
        }
    }
}
