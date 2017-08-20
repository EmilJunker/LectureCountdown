using System;
using System.Threading;

namespace CountdownLogic
{
    public static class Countdown
    {
        public static Tick tick = new Tick();

        private static Timer timer;

        private static AutoResetEvent countdownIsOver;

        private static bool isRunning;
        public static Boolean IsRunning
        {
            get { return isRunning; }
        }

        public static void TimerSetup(DateTime starttime, DateTime endtime)
        {
            try
            {
                timer.Dispose();
            }
            catch (NullReferenceException) { }

            tick.SetTime(starttime, endtime);

            countdownIsOver = new AutoResetEvent(false);

            timer = new Timer(tick.TimerTick, countdownIsOver, 0, 1000);

            isRunning = true;
        }

        public static void CountdownIsOver()
        {
            isRunning = false;
            tick.End();
            timer.Dispose();
        }
    }

    public delegate void TickEventHandler(object source, TickEventArgs e);

    public class TickEventArgs : EventArgs
    {
        private String countdown;
        public String Countdown
        {
            get { return countdown; }
        }

        private double progress;
        public double Progress
        {
            get { return progress; }
        }

        private String timeprogress;
        public String Timeprogress
        {
            get { return timeprogress; }
        }

        private String percentprogress;
        public String Percentprogress
        {
            get { return percentprogress; }
        }

        public TickEventArgs(String countdown, double progress, String timeprogress, String percentprogress)
        {
            this.countdown = countdown;
            this.progress = progress;
            this.timeprogress = timeprogress;
            this.percentprogress = percentprogress;
        }
    }

    public class Tick
    {
        public event TickEventHandler Ticked;

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

        public void SetTime(DateTime starttime, DateTime endtime)
        {
            this.starttime = starttime;
            this.endtime = endtime;

            this.duration = endtime.Subtract(starttime);

            this.durationString = " / " + duration.ToString(@"hh\:mm\:ss");
        }

        public void TimerTick(Object stateInfo)
        {
            currenttime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            timeleft = endtime.Subtract(currenttime);
            timepassed = duration.Subtract(timeleft);

            if (timeleft <= over)
            {
                Countdown.CountdownIsOver();
            }
            else
            {
                countdown = timeleft.ToString(@"hh\:mm\:ss");

                progress = timepassed.TotalSeconds / duration.TotalSeconds;

                timeprogress = timepassed.ToString(@"hh\:mm\:ss") + durationString;

                percentprogress = (progress * 100).ToString("0.00") + " %";

                Ticked(this, new TickEventArgs(countdown, progress, timeprogress, percentprogress));
            }
        }

        public void End()
        {
            countdown = "00:00:00";
            progress = 1.0;
            timeprogress = String.Empty;
            percentprogress = String.Empty;

            Ticked(this, new TickEventArgs(countdown, progress, timeprogress, percentprogress));
        }
    }
}
