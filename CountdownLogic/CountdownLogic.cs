using System;
using System.Threading;

namespace CountdownLogic {

    public static class Countdown {

        private static Timer timer;

        private static DateTime starttime;
        private static DateTime endtime;

        private static AutoResetEvent countdownIsOver;

        public static void TimerSetup(int hour, int min, Tick tick) {

            try {
                timer.Dispose();
            }
            catch (NullReferenceException) {

            }

            starttime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            endtime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 0);

            if ((hour < starttime.Hour) || (hour == starttime.Hour && min < starttime.Minute)) {
                endtime = endtime.AddDays(1);
            }

            tick.SetTime(starttime, endtime);

            countdownIsOver = new AutoResetEvent(false);

            timer = new Timer(tick.TimerTick, countdownIsOver, 0, 1000);

        }

        public static void CountdownIsOver() {
            timer.Dispose();
        }

    }

    public delegate void TickEventHandler(object source, TickEventArgs e);

    public delegate void EndEventHandler(object source, EndEventArgs e);

    public class TickEventArgs : EventArgs {

        private String countdown;
        public String Countdown {
            get { return countdown; }
        }

        private double progress;
        public double Progress {
            get { return progress; }
        }

        private String timeprogress;
        public String Timeprogress {
            get { return timeprogress; }
        }

        private String percentprogress;
        public String Percentprogress {
            get { return percentprogress; }
        }

        public TickEventArgs(String countdown, double progress, String timeprogress, String percentprogress) {
            this.countdown = countdown;
            this.progress = progress;
            this.timeprogress = timeprogress;
            this.percentprogress = percentprogress;
        }

    }

    public class EndEventArgs : EventArgs {

        private int secondsPassed;
        public int SecondsPassed {
            get { return secondsPassed; }
        }

        public EndEventArgs(TimeSpan timepassed) {
            this.secondsPassed = (int)Math.Round(timepassed.TotalSeconds,0);
        }

    }

    public class Tick {

        public event TickEventHandler Ticked;
        public event EndEventHandler Ended;

        private static TimeSpan over = new TimeSpan(0, 0, 0);

        private DateTime starttime;
        private DateTime endtime;
        private TimeSpan duration;
        private String durationString;

        private DateTime currenttime;
        private TimeSpan timeleft;
        private TimeSpan timepassed;

        private String countdown;
        private double progress;
        private String timeprogress;
        private String percentprogress;

        public void SetTime(DateTime starttime, DateTime endtime) {

            this.starttime = starttime;
            this.endtime = endtime;

            this.duration = endtime.Subtract(starttime);

            this.durationString = " / " + duration.ToString(@"hh\:mm\:ss");

            timeleft = duration;
            timepassed = new TimeSpan(0, 0, 0);

        }

        public void TimerTick(Object stateInfo) {

            currenttime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            timeleft = endtime.Subtract(currenttime);
            timepassed = duration.Subtract(timeleft);

            if (timeleft <= over) {

                Countdown.CountdownIsOver();
                Ended(this, new EndEventArgs(timepassed));

            }
            else {

                countdown = timeleft.ToString(@"hh\:mm\:ss");

                progress = timepassed.TotalSeconds / duration.TotalSeconds;

                timeprogress = timepassed.ToString(@"hh\:mm\:ss") + durationString;

                percentprogress = (progress * 100).ToString("0.00") + " %";

                Ticked(this, new TickEventArgs(countdown, progress, timeprogress, percentprogress));

            }

        }

    }
}
