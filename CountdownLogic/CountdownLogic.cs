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

        public static (DateTime starttime, DateTime endtime) TimerSetup(int length)
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

        public static int TimerSetup(DateTime starttime, DateTime endtime)
        {
            tick.SetTime(starttime, endtime);

            TimerSetup();

            return (int)endtime.Subtract(starttime).TotalMinutes;
        }

        public static void CountdownIsOver()
        {
            IsRunning = false;
            tick.End();
            timer.Dispose();
        }

        public static DateTime DateTimeNow()
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        }

        public static DateTime DateTimeToday(int hour, int minute, int second)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);
        }

        public static DateTime DateTimeTodayOrTomorrow(int hour, int minute, int second)
        {
            DateTime now = DateTimeNow();
            DateTime time = DateTimeToday(hour, minute, second);
            if ((hour < now.Hour) || (hour == now.Hour && minute < now.Minute))
            {
                time = time.AddDays(1);
            }
            return time;
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

        private string FormatTimeSpan(TimeSpan t)
        {
            return $"{(int)t.TotalHours:00}∶{t:mm}∶{t:ss}";
        }

        public void SetTime(DateTime start, DateTime end)
        {
            endtime = end;
            duration = end.Subtract(start);
            durationString = " / " + FormatTimeSpan(duration);
        }

        public void TimerTick(Object stateInfo)
        {
            currenttime = Countdown.DateTimeNow();
            timeleft = endtime.Subtract(currenttime);
            timepassed = duration.Subtract(timeleft);

            if (timeleft <= over)
            {
                Countdown.CountdownIsOver();
            }
            else
            {
                countdown = FormatTimeSpan(timeleft);
                progress = timepassed.TotalSeconds / duration.TotalSeconds;
                timeprogress = FormatTimeSpan(timepassed) + durationString;
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
