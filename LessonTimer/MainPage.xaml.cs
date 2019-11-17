﻿using CountdownLogic;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
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

            TimePicker.ClockIdentifier = SettingsPage.ClockFormat;

            switch (SettingsPage.CountdownBase)
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

                switch (SettingsPage.CountdownBase)
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
                suggestionsIterator = 0;
                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
                descriptionAutoSetLock = true;

                switch (SettingsPage.CountdownBase)
                {
                    case "time":
                        Tuple<TimeSpan, string> endTimeSuggestion = GetEndTimeSuggestion();
                        TimePicker.Time = endTimeSuggestion.Item1;
                        nextDescription = endTimeSuggestion.Item2;
                        break;
                    case "length":
                        Tuple<double, string> lengthSuggestion = GetLengthSuggestion();
                        LengthPicker.Text = lengthSuggestion.Item1.ToString();
                        nextDescription = lengthSuggestion.Item2;
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

        void LengthPicker_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                StartCountdown();
            }
        }

        void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartCountdown();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Countdown.CountdownIsOver();

            endtime = DateTime.Now;

            CancelToastNotification();

            StartButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void StartCountdown()
        {
            bool success = false;

            switch (SettingsPage.CountdownBase)
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
                        double length = Convert.ToInt32(LengthPicker.Text);
                        if (0 < length && length < 1440)
                        {
                            Tuple<DateTime, DateTime> times = Countdown.TimerSetup(length);
                            starttime = times.Item1;
                            endtime = times.Item2;
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

                if (SettingsPage.NotificationsEnabled)
                {
                    ScheduleToastNotification(SettingsPage.NotificationSoundEnabled);
                }
            }
        }

        async void UpdateCountdown(object source, TickEventArgs e)
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

        public static List<string> emoji = new List<string>(new string[] { "\U0001F600", "\U0001F601", "\U0001F602", "\U0001F923", "\U0001F603", "\U0001F604", "\U0001F605", "\U0001F606",
            "\U0001F609", "\U0001F60A", "\U0001F60B", "\U0001F60E", "\U0001F60D", "\U0000263A", "\U0001F642", "\U0001F917", "\U0001F914", "\U0001F610", "\U0001F611", "\U0001F636",
            "\U0001F644", "\U0001F60F", "\U0001F623", "\U0001F625", "\U0001F62E", "\U0001F910", "\U0001F615", "\U0001F643", "\U0001F62F", "\U0001F62A", "\U0001F62B", "\U0001F634",
            "\U0001F60C", "\U0001F61B", "\U0001F61C", "\U0001F61D", "\U0001F924", "\U0001F612", "\U0001F613", "\U0001F614", "\U0001F615", "\U0001F643", "\U0001F632", "\U00002639",
            "\U0001F641", "\U0001F616", "\U0001F61E", "\U0001F61F", "\U0001F624", "\U0001F622", "\U0001F62D", "\U0001F626", "\U0001F627", "\U0001F628", "\U0001F629", "\U0001F62C",
            "\U0001F630", "\U0001F631", "\U0001F633", "\U0001F635", "\U0001F621", "\U0001F620", "\U0001F921", "\U0001F913", "\U0001F4A9", "\U0001F648", "\U0001F649", "\U0001F64A" });

        public static ScheduledToastNotification toast;

        public static void ScheduleToastNotification(bool sound)
        {
            CancelToastNotification();

            double durationTotalSeconds = endtime.Subtract(starttime).TotalSeconds;

            if (durationTotalSeconds > 0)
            {
                Random rand = new Random();
                int index = rand.Next(0, 68);

                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

                string title = loader.GetString("NotificationTitle");
                string content = loader.GetString("NotificationText1") + durationTotalSeconds.ToString() + loader.GetString("NotificationText2") + " " + emoji[index];
                string silent = sound ? "false" : "true";

                string toastXmlString =
                $@"<toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{content}</text>
                        </binding>
                    </visual>
                    <audio silent='{silent}'/>
                </toast>";

                XmlDocument toastXml = new XmlDocument();
                toastXml.LoadXml(toastXmlString);
                toast = new ScheduledToastNotification(toastXml, endtime);

                ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
            }
        }

        public static void CancelToastNotification()
        {
            try
            {
                ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(toast);
            }
            catch (Exception) { }
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

            switch (SettingsPage.CountdownBase)
            {
                case "time":
                    Tuple<TimeSpan, string> endTimeSuggestion = GetEndTimeSuggestion();
                    TimePicker.Time = endTimeSuggestion.Item1;
                    description = endTimeSuggestion.Item2;
                    break;
                case "length":
                    Tuple<double, string> lengthSuggestion = GetLengthSuggestion();
                    LengthPicker.Text = lengthSuggestion.Item1.ToString();
                    description = lengthSuggestion.Item2;
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

                switch (SettingsPage.CountdownBase)
                {
                    case "time":
                        Tuple<TimeSpan, string> endTimeSuggestion = GetEndTimeSuggestion(allAppointments);
                        if (endTimeSuggestion != null)
                        {
                            TimePicker.Time = endTimeSuggestion.Item1;
                            description = endTimeSuggestion.Item2;
                        }
                        break;
                    case "length":
                        Tuple<double, string> lengthSuggestion = GetLengthSuggestion(allAppointments);
                        if (lengthSuggestion != null)
                        {
                            LengthPicker.Text = lengthSuggestion.Item1.ToString();
                            description = lengthSuggestion.Item2;
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

        private static int suggestionsIterator = 0;
        private static int calendarSuggestionsIterator = 0;

        private static Tuple<double, string> GetLengthSuggestion()
        {
            double length = SettingsPage.LectureLengths[suggestionsIterator];

            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string description = length.ToString() + loader.GetString("MinuteLecture");

            suggestionsIterator++;
            if (suggestionsIterator >= SettingsPage.LectureLengths.Count)
            {
                suggestionsIterator = 0;
            }

            return new Tuple<double, string>(length, description);
        }

        private static Tuple<TimeSpan, string> GetEndTimeSuggestion()
        {
            DateTime time = DateTime.Now.AddMinutes(SettingsPage.LectureLengths[suggestionsIterator]);
            TimeSpan span = TimeSpan.FromMinutes(SettingsPage.LectureLengthRoundTo);

            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string description = SettingsPage.LectureLengths[suggestionsIterator].ToString() + loader.GetString("MinuteLecture");

            suggestionsIterator++;
            if (suggestionsIterator >= SettingsPage.LectureLengths.Count)
            {
                suggestionsIterator = 0;
            }

            TimeSpan t = (time.Subtract(DateTime.MinValue)).Add(new TimeSpan(0, span.Minutes / 2, 0));
            DateTime newTime = DateTime.MinValue.Add(new TimeSpan(0, (((int)t.TotalMinutes) / (int)span.TotalMinutes) * span.Minutes, 0));

            return new Tuple<TimeSpan, string>(new TimeSpan(newTime.Hour, newTime.Minute, 0), description);
        }

        private static Tuple<double, string> GetLengthSuggestion(IReadOnlyList<Appointment> allAppointments)
        {
            List<Appointment> appointments = new List<Appointment>();

            foreach (Appointment a in allAppointments)
            {
                if (!a.AllDay)
                {
                    appointments.Add(a);
                }
            }

            Appointment nextAppointment;

            try
            {
                nextAppointment = appointments[calendarSuggestionsIterator];
            }
            catch (ArgumentOutOfRangeException)
            {
                calendarSuggestionsIterator = 0;

                try
                {
                    nextAppointment = appointments[calendarSuggestionsIterator];
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }

            calendarSuggestionsIterator++;

            double nextAppointmentLength = nextAppointment.Duration.TotalMinutes;

            if (SettingsPage.AcademicQuarterBeginEnabled && SettingsPage.AcademicQuarterEndEnabled)
            {
                if (nextAppointmentLength > 30)
                {
                    nextAppointmentLength -= 30;
                }
            }
            else if (SettingsPage.AcademicQuarterBeginEnabled || SettingsPage.AcademicQuarterEndEnabled)
            {
                if (nextAppointmentLength > 15)
                {
                    nextAppointmentLength -= 15;
                }
            }

            return new Tuple<double, string>(nextAppointmentLength, nextAppointment.Subject);
        }

        private static Tuple<TimeSpan, string> GetEndTimeSuggestion(IReadOnlyList<Appointment> allAppointments)
        {
            List<Appointment> appointments = new List<Appointment>();

            foreach (Appointment a in allAppointments)
            {
                if (!a.AllDay)
                {
                    appointments.Add(a);
                }
            }

            Appointment nextAppointment;

            try
            {
                nextAppointment = appointments[calendarSuggestionsIterator];
            }
            catch (ArgumentOutOfRangeException)
            {
                calendarSuggestionsIterator = 0;

                try
                {
                    nextAppointment = appointments[calendarSuggestionsIterator];
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }
            }

            calendarSuggestionsIterator++;

            DateTimeOffset nextAppointmentEndTime = nextAppointment.StartTime.Add(nextAppointment.Duration);

            if (SettingsPage.AcademicQuarterEndEnabled && nextAppointment.Duration > TimeSpan.FromMinutes(15))
            {
                nextAppointmentEndTime = nextAppointmentEndTime.AddMinutes(-15);
            }

            return new Tuple<TimeSpan, string>(new TimeSpan(nextAppointmentEndTime.Hour, nextAppointmentEndTime.Minute, 0), nextAppointment.Subject);
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
            bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);

            ControlPanel.Visibility = Visibility.Collapsed;
            compactMode = true;
        }

        private async void CompactOverlayOff()
        {
            bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

            ControlPanel.Visibility = Visibility.Visible;
            compactMode = false;
        }
    }
}
