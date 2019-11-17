using CountdownLogic;
using LessonTimer.Services;
using System;
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

        public static string currentDescription;
        public static string nextDescription;
        public static bool descriptionAutoSetLock;

        public static DateTime starttime;
        public static DateTime endtime;

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
                case "time":
                    LengthPicker.Visibility = Visibility.Collapsed;
                    TimePicker.Visibility = Visibility.Visible;
                    break;
                case "length":
                    LengthPicker.Visibility = Visibility.Visible;
                    TimePicker.Visibility = Visibility.Collapsed;
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

            Size size = new Size(400, 300);
            ApplicationView.GetForCurrentView().SetPreferredMinSize(size);

            Countdown.tick.Ticked += new TickEventHandler(UpdateCountdown);

            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

            this.Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StartButton.Focus(FocusState.Programmatic);

            if (endtime > DateTime.Now)
            {
                Countdown.TimerSetup(starttime, endtime);

                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;

                DisplayMessage(currentDescription, false);

                switch (Settings.CountdownBase)
                {
                    case "time":
                        TimePicker.Time = new TimeSpan(endtime.Hour, endtime.Minute, 0);
                        break;
                    case "length":
                        LengthPicker.Text = Math.Floor(endtime.Subtract(starttime).TotalMinutes).ToString();
                        break;
                }
            }
            else
            {
                TimeSuggestions.SuggestionsIterator = 0;
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
                descriptionAutoSetLock = true;

                switch (Settings.CountdownBase)
                {
                    case "time":
                        (TimePicker.Time, nextDescription) = TimeSuggestions.GetEndTimeSuggestion();
                        break;
                    case "length":
                        (LengthPicker.Text, nextDescription) = TimeSuggestions.GetLengthSuggestion();
                        break;
                }
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
            double length = 0;
            bool success = false;

            switch (Settings.CountdownBase)
            {
                case "time":
                    int hour = TimePicker.Time.Hours;
                    int min = TimePicker.Time.Minutes;

                    starttime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                    endtime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 0);

                    if ((hour < starttime.Hour) || (hour == starttime.Hour && min < starttime.Minute))
                    {
                        endtime = endtime.AddDays(1);
                    }

                    Countdown.TimerSetup(starttime, endtime);
                    success = true;
                    break;

                case "length":
                    try
                    {
                        length = Convert.ToInt32(LengthPicker.Text);
                        if (0 < length && length < 1440)
                        {
                            (MainPage.starttime, MainPage.endtime) = Countdown.TimerSetup(length);
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
            }

            if (success)
            {
                currentDescription = nextDescription;
                nextDescription = String.Empty;
                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                CancelButton.Focus(FocusState.Programmatic);

                DisplayMessage(currentDescription, false);

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
                CountdownLabel.Text = (e.Countdown);
                CountdownProgressBar.Value = (e.Progress);
                CountdownProgressText.Text = (e.Timeprogress);
                CountdownProgressPercentage.Text = (e.Percentprogress);

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
            Notifications.ScheduleToastNotification(Settings.NotificationSoundEnabled, starttime, endtime);
        }

        public static void CancelNotification()
        {
            Notifications.CancelToastNotification();
        }

        private void LengthPicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (descriptionAutoSetLock)
            {
                descriptionAutoSetLock = false;
            }
            else
            {
                nextDescription = String.Empty;
                StartButton.IsEnabled = true;
            }
        }

        private void TimePicker_Changed(object sender, TimePickerValueChangedEventArgs e)
        {
            if (descriptionAutoSetLock)
            {
                descriptionAutoSetLock = false;
            }
            else if (e.NewTime != e.OldTime)
            {
                nextDescription = String.Empty;
                StartButton.IsEnabled = true;
            }
        }

        private void SuggestButton_Click(object sender, RoutedEventArgs e)
        {
            string description = null;
            descriptionAutoSetLock = true;

            switch (Settings.CountdownBase)
            {
                case "time":
                    (TimePicker.Time, description) = TimeSuggestions.GetEndTimeSuggestion();
                    break;
                case "length":
                    (LengthPicker.Text, description) = TimeSuggestions.GetLengthSuggestion();
                    break;
            }

            if (Countdown.IsRunning)
            {
                nextDescription = description;
                StartButton.IsEnabled = true;
            }
            else
            {
                nextDescription = description;
                DisplayMessage(description, true);
            }
        }

        private async void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            DateTimeOffset dateToShow = DateTime.Now;
            TimeSpan duration = TimeSpan.FromHours(24);

            string description = null;
            descriptionAutoSetLock = true;

            try
            {
                var allAppointments = await appointmentStore.FindAppointmentsAsync(dateToShow, duration);

                switch (Settings.CountdownBase)
                {
                    case "time":
                        var endTimeSuggestion = TimeSuggestions.GetEndTimeSuggestion(allAppointments);
                        if (endTimeSuggestion.description != null)
                        {
                            TimePicker.Time = endTimeSuggestion.endtime;
                            description = endTimeSuggestion.description;
                        }
                        break;
                    case "length":
                        var lengthSuggestion = TimeSuggestions.GetLengthSuggestion(allAppointments);
                        if (lengthSuggestion.description != null)
                        {
                            LengthPicker.Text = lengthSuggestion.length;
                            description = lengthSuggestion.description;
                        }
                        break;
                }

                if (description != null)
                {
                    if (Countdown.IsRunning)
                    {
                        nextDescription = description;
                        StartButton.IsEnabled = true;
                    }
                    else
                    {
                        nextDescription = description;
                        DisplayMessage(description, true);
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
    }
}
