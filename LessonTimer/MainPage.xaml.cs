using System;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CountdownLogic;
using Windows.Globalization;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Navigation;

namespace LessonTimer {

    public sealed partial class MainPage : Page {

        Boolean compactMode;

        public MainPage() {

            this.InitializeComponent();

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(MainTitleBar);

            var userFormat = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code });
            var timeFormat = userFormat.Format(DateTime.Now);

            if ((timeFormat.Contains("AM")) || (timeFormat.Contains("PM")) || (timeFormat.Contains("am")) || (timeFormat.Contains("pm"))) {
                TimePicker.ClockIdentifier = "12HourClock";
            }
            else {
                TimePicker.ClockIdentifier = "24HourClock";
            }

            Tuple<TimeSpan, string> endTimeSuggestion = GetEndTimeSuggestion();

            TimePicker.Time = endTimeSuggestion.Item1;
            currentDescription = endTimeSuggestion.Item2;

            try {
                if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay)) {
                    CompactOverlayButton.Visibility = Visibility.Visible;
                }
                else {
                    CompactOverlayButton.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception) {
                CompactOverlayButton.Visibility = Visibility.Collapsed;
            }

            compactMode = false;

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(400, 300));

            Countdown.tick.Ticked += new TickEventHandler(UpdateCountdown);

            this.Loaded += Page_Loaded;

        }

        void Page_Loaded(object sender, RoutedEventArgs e) {

            StartButton.Focus(FocusState.Programmatic);

            if (endtime > DateTime.Now) {

                Countdown.TimerSetup(starttime, endtime);

                FadeInStoryboard.Begin();
                InfoTextBlock.Text = currentDescription;
                ToolTipService.SetToolTip(InfoTextBlock, currentDescription);
                TimePicker.Time = new TimeSpan(endtime.Hour, endtime.Minute, 0);
                StartButton.IsEnabled = false;
                CancelButton.IsEnabled = true;

            }
            else {

                StartButton.IsEnabled = true;
                CancelButton.IsEnabled = false;

            }

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e) {
            await BackgroundExecutionManager.RequestAccessAsync();
        }

        static List<String> emoji = new List<String>(new String[] { "\U0001F600", "\U0001F601", "\U0001F602", "\U0001F923", "\U0001F603", "\U0001F604", "\U0001F605", "\U0001F606",
            "\U0001F609", "\U0001F60A", "\U0001F60B", "\U0001F60E", "\U0001F60D", "\U0000263A", "\U0001F642", "\U0001F917", "\U0001F914", "\U0001F610", "\U0001F611", "\U0001F636",
            "\U0001F644", "\U0001F60F", "\U0001F623", "\U0001F625", "\U0001F62E", "\U0001F910", "\U0001F615", "\U0001F643", "\U0001F62F", "\U0001F62A", "\U0001F62B", "\U0001F634",
            "\U0001F60C", "\U0001F61B", "\U0001F61C", "\U0001F61D", "\U0001F924", "\U0001F612", "\U0001F613", "\U0001F614", "\U0001F615", "\U0001F643", "\U0001F632", "\U00002639",
            "\U0001F641", "\U0001F616", "\U0001F61E", "\U0001F61F", "\U0001F624", "\U0001F622", "\U0001F62D", "\U0001F626", "\U0001F627", "\U0001F628", "\U0001F629", "\U0001F62C",
            "\U0001F630", "\U0001F631", "\U0001F633", "\U0001F635", "\U0001F621", "\U0001F620", "\U0001F921", "\U0001F913", "\U0001F4A9", "\U0001F648", "\U0001F649", "\U0001F64A" });

        public static ScheduledToastNotification toast;

        public static String currentDescription;

        public static DateTime starttime;
        public static DateTime endtime;

        void StartButton_Click(object sender, RoutedEventArgs e) {

            int hour = TimePicker.Time.Hours;
            int min = TimePicker.Time.Minutes;

            starttime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            endtime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 0);

            if ((hour < starttime.Hour) || (hour == starttime.Hour && min < starttime.Minute)) {
                endtime = endtime.AddDays(1);
            }

            Countdown.TimerSetup(starttime, endtime);

            FadeInStoryboard.Begin();
            InfoTextBlock.Text = currentDescription;
            ToolTipService.SetToolTip(InfoTextBlock, currentDescription);
            StartButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            double durationTotalSeconds = endtime.Subtract(starttime).TotalSeconds;

            try {
                ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(toast);
            }
            catch (Exception) { }

            if (durationTotalSeconds > 0) {

                Random rand = new Random();
                int index = rand.Next(0, 68);

                string title = "It's over!";
                string content = System.String.Format("A total of {0} seconds have passed {1}", durationTotalSeconds, emoji[index]);

                string toastXmlString =
                $@"<toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{content}</text>
                        </binding>
                    </visual>
                    <audio silent='true'/>
                </toast>";

                XmlDocument toastXml = new XmlDocument();
                toastXml.LoadXml(toastXmlString);
                MainPage.toast = new ScheduledToastNotification(toastXml, endtime);

                ToastNotificationManager.CreateToastNotifier().AddToSchedule(MainPage.toast);

            }

        }

        async void UpdateCountdown(object source, TickEventArgs e) {

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {

                CountdownLabel.Text = (e.Countdown);
                CountdownProgressBar.Value = (e.Progress);
                CountdownProgressText.Text = (e.Timeprogress);
                CountdownProgressPercentage.Text = (e.Percentprogress);

                if (!Countdown.IsRunning) {
                    StartButton.IsEnabled = true;
                    CancelButton.IsEnabled = false;
                    InfoTextBlock.Text = String.Empty;
                    ToolTipService.SetToolTip(InfoTextBlock, null);
                }

            });

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {

            Countdown.CountdownIsOver();

            endtime = DateTime.Now;

            try {
                ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(toast);
            }
            catch (Exception) { }

            StartButton.IsEnabled = true;
            CancelButton.IsEnabled = false;

        }

        void TimePicker_Changed(object sender, TimePickerValueChangedEventArgs e) {

            if (e.NewTime != e.OldTime) {

                if (Countdown.IsRunning) {
                    currentDescription = String.Empty;
                }
                else {
                    currentDescription = String.Empty;
                    InfoTextBlock.Text = String.Empty;
                }

                StartButton.IsEnabled = true;

            }

        }

        void SuggestButton_Click(object sender, RoutedEventArgs e) {

            Tuple<TimeSpan, string> endTimeSuggestion = GetEndTimeSuggestion();

            TimePicker.Time = endTimeSuggestion.Item1;

            if (Countdown.IsRunning) {
                currentDescription = endTimeSuggestion.Item2;
            }
            else {
                currentDescription = endTimeSuggestion.Item2;
                InfoTextBlock.Text = currentDescription;
                InfoTextBlock.Opacity = 1.0;
                FadeOutStoryboard.Begin();
            }

        }

        async void CalendarButton_Click(object sender, RoutedEventArgs e) {

            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            DateTimeOffset dateToShow = DateTime.Now;
            TimeSpan duration = TimeSpan.FromHours(24);

            try {

                var allAppointments = await appointmentStore.FindAppointmentsAsync(dateToShow, duration);

                Tuple<TimeSpan, string> endTimeSuggestion = GetEndTimeSuggestion(allAppointments);

                if (endTimeSuggestion != null) {

                    TimePicker.Time = endTimeSuggestion.Item1;

                    if (Countdown.IsRunning) {
                        currentDescription = endTimeSuggestion.Item2;
                    }
                    else {
                        currentDescription = endTimeSuggestion.Item2;
                        InfoTextBlock.Text = currentDescription;
                        InfoTextBlock.Opacity = 1.0;
                        FadeOutStoryboard.Begin();
                    }

                }
                else {

                    if (Countdown.IsRunning) {
                        currentDescription = String.Empty;
                    }
                    else {
                        currentDescription = String.Empty;
                        InfoTextBlock.Text = "No events today in you calendar";
                        InfoTextBlock.Opacity = 1.0;
                        FadeOutStoryboard.Begin();
                    }

                }

            }
            catch (System.NullReferenceException) {

                if (Countdown.IsRunning) {
                    currentDescription = String.Empty;
                }
                else {
                    currentDescription = String.Empty;
                    InfoTextBlock.Text = "Please grant access permission to calendar";
                    InfoTextBlock.Opacity = 1.0;
                    FadeOutStoryboard.Begin();
                }

            }

        }

        static List<int> suggestions = new List<int>(new int[] { 90, 150, 195, 300 });
        static int suggestionsIterator = 0;
        static int calendarSuggestionsIterator = 0;

        public static Tuple<TimeSpan, string> GetEndTimeSuggestion() {

            DateTime time = DateTime.Now.AddMinutes(suggestions[suggestionsIterator]);
            TimeSpan span = TimeSpan.FromMinutes(15);

            DateTime newTime;

            string description = suggestions[suggestionsIterator].ToString() + "-minute lecture";

            suggestionsIterator++;
            if (suggestionsIterator >= suggestions.Count) {
                suggestionsIterator = 0;
            }

            var delta = time.Ticks % span.Ticks;
            bool roundUp = delta > span.Ticks / 2;

            if (roundUp) {
                newTime = new DateTime(((time.Ticks + span.Ticks - 1) / span.Ticks) * span.Ticks);
            }
            else {
                newTime = time.AddMinutes(-(time.Minute % 15));
            }

            var endTimeSuggestion = new Tuple<TimeSpan, string>(new TimeSpan(newTime.Hour, newTime.Minute, 0), description);
            return endTimeSuggestion;

        }

        public static Tuple<TimeSpan, string> GetEndTimeSuggestion(IReadOnlyList<Appointment> allAppointments) {
            
            List<Appointment> appointments = new List<Appointment>();

            foreach (Appointment a in allAppointments) {
                if (!a.AllDay) {
                    appointments.Add(a);
                }
            }

            Tuple<TimeSpan, string> endTimeSuggestion;
            Appointment nextAppointment;

            try {
                nextAppointment = appointments[calendarSuggestionsIterator];
            }
            catch (ArgumentOutOfRangeException) {

                calendarSuggestionsIterator = 0;

                try {
                    nextAppointment = appointments[calendarSuggestionsIterator];
                }
                catch (ArgumentOutOfRangeException) {
                    return null;
                }

            }

            calendarSuggestionsIterator++;

            DateTimeOffset nextAppointmentEndTime = nextAppointment.StartTime.Add(nextAppointment.Duration);
            endTimeSuggestion = new Tuple<TimeSpan, string>(new TimeSpan(nextAppointmentEndTime.Hour, nextAppointmentEndTime.Minute, 0), nextAppointment.Subject);
            return endTimeSuggestion;

        }

        void CompactOverlayButton_Click(object sender, RoutedEventArgs e) {

            if (compactMode) {
                CompactOverlayOff();
            }
            else {
                CompactOverlayOn();
            }

        }

        async void CompactOverlayOn() {

            ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            compactOptions.CustomSize = new Windows.Foundation.Size(320, 160);
            bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);

            ControlPanel.Visibility = Visibility.Collapsed;
            compactMode = true;

        }

        async void CompactOverlayOff() {

            bool modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

            ControlPanel.Visibility = Visibility.Visible;
            compactMode = false;

        }

    }
}
