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

namespace LessonTimer {

    public sealed partial class MainPage : Page {

        Boolean compactMode;

        Tick tick;

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

            TimePickerEndTimeSuggestion();

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

            this.Loaded += Page_Loaded;

        }

        void Page_Loaded(object sender, RoutedEventArgs e) {
            StartButton.Focus(FocusState.Programmatic);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) {

            tick = new Tick();
            tick.Ticked += new TickEventHandler(UpdateCountdown);
            tick.Ended += new EndEventHandler(EndCountdown);

            Countdown.TimerSetup(TimePicker.Time.Hours, TimePicker.Time.Minutes, tick);

            StartButton.IsEnabled = false;

        }

        async void UpdateCountdown(object source, TickEventArgs e) {

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                CountdownLabel.Text = (e.Countdown);
                CountdownProgressBar.Value = (e.Progress);
                CountdownProgressText.Text = (e.Timeprogress);
                CountdownProgressPercentage.Text = (e.Percentprogress);
            });

        }

        async void EndCountdown(object source, EndEventArgs e) {

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                CountdownLabel.Text = "00:00:00";
                CountdownProgressBar.Value = 1;
                CountdownProgressText.Text = String.Empty;
                CountdownProgressPercentage.Text = String.Empty;
                StartButton.IsEnabled = true;
            });

            string title = "It's over!";
            string content = System.String.Format("A total of {0} seconds have passed" , e.SecondsPassed);

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

            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);

        }

        void TimePicker_Changed(object sender, TimePickerValueChangedEventArgs e) {

            if (e.NewTime != e.OldTime) {
                InfoTextBlock.Text = String.Empty;
                StartButton.IsEnabled = true;
            }

        }

        private void SuggestButton_Click(object sender, RoutedEventArgs e) {
            TimePickerEndTimeSuggestion();
        }

        void TimePickerEndTimeSuggestion() {

            DateTime time = DateTime.Now.AddMinutes(90);
            TimeSpan span = TimeSpan.FromMinutes(15);

            DateTime newTime;

            var delta = time.Ticks % span.Ticks;
            bool roundUp = delta > span.Ticks / 2;

            if (roundUp) {
                newTime = new DateTime(((time.Ticks + span.Ticks - 1) / span.Ticks) * span.Ticks);
            }
            else {
                newTime = time.AddMinutes(-(time.Minute % 15));
            }

            TimePicker.Time = new TimeSpan(newTime.Hour, newTime.Minute, 0);

        }

        int calendarIterator = 0;

        async void CalendarButton_Click(object sender, RoutedEventArgs e) {

            AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

            DateTimeOffset dateToShow = DateTime.Now;
            TimeSpan duration = TimeSpan.FromHours(24);

            var appointments = await appointmentStore.FindAppointmentsAsync(dateToShow, duration);

            try {
                Appointment nextAppointment = appointments[calendarIterator];
                calendarIterator++;
                if (calendarIterator >= appointments.Count) {
                    calendarIterator = 0;
                }
                DateTimeOffset nextAppointmentEndTime = nextAppointment.StartTime.Add(nextAppointment.Duration);
                TimePicker.Time = new TimeSpan(nextAppointmentEndTime.Hour, nextAppointmentEndTime.Minute, 0);
                InfoTextBlock.Text = nextAppointment.Subject;
            }
            catch (Exception) {
                calendarIterator = 0;
                InfoTextBlock.Text = String.Empty;
            }

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
