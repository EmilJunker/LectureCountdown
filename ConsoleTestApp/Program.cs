using CountdownLogic;
using System;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            int? arg0 = null;
            int? arg1 = null;
            int? arg2 = null;
            int? arg3 = null;
            int? arg4 = null;
            int? arg5 = null;

            try
            {
                arg0 = int.Parse(args[0]);
                arg1 = int.Parse(args[1]);
                arg2 = int.Parse(args[2]);
                arg3 = int.Parse(args[3]);
                arg4 = int.Parse(args[4]);
                arg5 = int.Parse(args[5]);
            }
            catch (Exception) { }

            DateTime starttime = Countdown.DateTimeNow();

            if (arg0 is null)
            {
                // use default length 10 minutes
                Countdown.TimerSetup(starttime, 10);
            }
            else if (arg1 is null)
            {
                // have arg0 -> use as length
                Countdown.TimerSetup(starttime, (int)arg0);
            }
            else if (arg2 is null)
            {
                // have arg0 arg1 -> use as endtime today or tomorrow
                DateTime endtime = Countdown.DateTimeTodayOrTomorrow((int)arg0, (int)arg1, 0);
                Countdown.TimerSetup(starttime, endtime);
            }
            else if (arg3 is null)
            {
                // have arg0 arg1 arg2 -> use as endtime
                DateTime endtime = Countdown.DateTimeToday((int)arg1, (int)arg2, 0);
                endtime = endtime.AddDays((int)arg0);
                Countdown.TimerSetup(starttime, endtime);
            }
            else if (arg4 is null)
            {
                // have arg0 arg1 arg2 arg3 -> use as starttime + arg3 as length
                starttime = Countdown.DateTimeToday((int)arg1, (int)arg2, 0);
                starttime = starttime.AddDays((int)arg0);
                Countdown.TimerSetup(starttime, (int)arg3);
            }
            else if (!(arg5 is null))
            {
                // have all -> use as starttime + endtime
                starttime = Countdown.DateTimeToday((int)arg1, (int)arg2, 0);
                starttime = starttime.AddDays((int)arg0);
                DateTime endtime = Countdown.DateTimeToday((int)arg4, (int)arg5, 0);
                endtime = endtime.AddDays((int)arg3);
                Countdown.TimerSetup(starttime, endtime);
            }

            Countdown.tick.Ticked += new TickEventHandler(UpdateCountdown);

            Console.ReadLine();
        }

        static void UpdateCountdown(object source, TickEventArgs e)
        {
            Console.Clear();

            Console.WriteLine(e.Countdown);
            Console.WriteLine(e.Timeprogress);
            Console.WriteLine(e.Percentprogress);

            int i = 0;
            double limit = Math.Floor((e.Progress * 100));

            while (i < limit)
            {
                Console.Write("#");
                i++;
            }

            while (i < 100)
            {
                Console.Write("-");
                i++;
            }

            if (!Countdown.IsRunning)
            {
                Console.Clear();

                Console.WriteLine("The countdown is over");
            }
        }
    }
}
