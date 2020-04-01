using System;
using System.Threading;

namespace CountdownLogic
{
    public static class Countdown
    {
        public static Tick tick = new Tick();
        public static bool IsRunning { get; private set; }

        private static Timer timer;
        private static AutoResetEvent countdownIsOver;

        private static void TimerSetup()
        {
            try
            {
                timer.Dispose();
            }
            catch (NullReferenceException) { }

            countdownIsOver = new AutoResetEvent(false);

            timer = new Timer(tick.TimerTick, countdownIsOver, 0, 1000);

            IsRunning = true;
        }

        public static (DateTime starttime, DateTime endtime) TimerSetup(double length)
        {
            DateTime starttime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            DateTime endtime;
            try
            {
                endtime = starttime.AddMinutes(length);
            }
            catch (ArgumentOutOfRangeException)
            {
                endtime = DateTime.MaxValue;
            }

            tick.SetTime(starttime, endtime);

            TimerSetup();

            return (starttime, endtime);
        }

        public static void TimerSetup(DateTime starttime, DateTime endtime)
        {
            tick.SetTime(starttime, endtime);

            TimerSetup();
        }

        public static void CountdownIsOver()
        {
            IsRunning = false;
            tick.End();
            timer.Dispose();
        }
    }

    public delegate void TickEventHandler(object source, TickEventArgs e);

    public class TickEventArgs : EventArgs
    {
        public string Countdown { get; }
        public double Progress { get; }
        public string Timeprogress { get; }
        public string Percentprogress { get; }

        public TickEventArgs(string countdown, double progress, string timeprogress, string percentprogress)
        {
            Countdown = countdown;
            Progress = progress;
            Timeprogress = timeprogress;
            Percentprogress = percentprogress;
        }
    }

    public class Tick
    {
        public event TickEventHandler Ticked;

        private static readonly TimeSpan over = new TimeSpan(0, 0, 0);

        private DateTime endtime;
        private TimeSpan duration;
        private string durationString;

        private DateTime currenttime;
        private TimeSpan timeleft;
        private TimeSpan timepassed;

        private string countdown;
        private double progress;
        private string timeprogress;
        private string percentprogress;

        public void SetTime(DateTime starttime, DateTime endtime)
        {
            this.endtime = endtime;
            this.duration = endtime.Subtract(starttime);
            this.durationString = " / " + duration.ToString(@"hh\∶mm\∶ss");
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
                countdown = timeleft.ToString(@"hh\∶mm\∶ss");
                progress = timepassed.TotalSeconds / duration.TotalSeconds;
                timeprogress = timepassed.ToString(@"hh\∶mm\∶ss") + durationString;
                percentprogress = (progress * 100).ToString("0.00") + " %";

                Ticked(this, new TickEventArgs(countdown, progress, timeprogress, percentprogress));
            }
        }

        public void End()
        {
            countdown = "00∶00∶00";
            progress = 1.0;
            timeprogress = String.Empty;
            percentprogress = String.Empty;

            Ticked(this, new TickEventArgs(countdown, progress, timeprogress, percentprogress));
        }
    }
}
