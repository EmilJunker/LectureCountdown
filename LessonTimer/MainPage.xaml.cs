using System;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CountdownLogic;
using Windows.Globalization;

namespace LessonTimer {

    public sealed partial class MainPage : Page {

        Boolean compactMode;
        Boolean countdownRunning;

        int endH;
        int endM;

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

            int nextHour = DateTime.Now.AddMinutes(42).Hour + 1;
            TimePicker.Time = new TimeSpan(nextHour, 0, 0);

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
            countdownRunning = false;

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(400, 300));

            this.Loaded += Page_Loaded;

        }

        void Page_Loaded(object sender, RoutedEventArgs e) {
            StartButton.Focus(FocusState.Programmatic);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) {

            if (endH != TimePicker.Time.Hours || endM != TimePicker.Time.Minutes || !countdownRunning) {

                countdownRunning = true;

                endH = TimePicker.Time.Hours;
                endM = TimePicker.Time.Minutes;

                tick = new Tick();
                tick.Ticked += new TickEventHandler(UpdateCountdown);

                Countdown.TimerSetup(endH, endM, tick);

            }

        }

        async void UpdateCountdown(object source, TickEventArgs e) {

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                CountdownLabel.Text = (e.Countdown);
                CountdownProgressBar.Value = (e.Progress);
                CountdownProgressText.Text = (e.Timeprogress);
                CountdownProgressPercentage.Text = (e.Percentprogress);
            });

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
