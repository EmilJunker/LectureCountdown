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
                Countdown.TimerSetup(starttime, arg0.Value);
            }
            else if (arg2 is null)
            {
                // have arg0 arg1 -> use as endtime today or tomorrow
                DateTime endtime = Countdown.DateTimeTodayOrTomorrow(arg0.Value, arg1.Value, 0);
                Countdown.TimerSetup(starttime, endtime);
            }
            else if (arg3 is null)
            {
                // have arg0 arg1 arg2 -> use as endtime
                DateTime endtime = Countdown.DateTimeToday(arg1.Value, arg2.Value, 0);
                endtime = endtime.AddDays(arg0.Value);
                Countdown.TimerSetup(starttime, endtime);
            }
            else if (arg4 is null)
            {
                // have arg0 arg1 arg2 arg3 -> use as starttime + arg3 as length
                starttime = Countdown.DateTimeToday(arg1.Value, arg2.Value, 0);
                starttime = starttime.AddDays(arg0.Value);
                Countdown.TimerSetup(starttime, arg3.Value);
            }
            else if (!(arg5 is null))
            {
                // have all -> use as starttime + endtime
                starttime = Countdown.DateTimeToday(arg1.Value, arg2.Value, 0);
                starttime = starttime.AddDays(arg0.Value);
                DateTime endtime = Countdown.DateTimeToday(arg4.Value, arg5.Value, 0);
                endtime = endtime.AddDays(arg3.Value);
                Countdown.TimerSetup(starttime, endtime);
            }

            Countdown.tick.Ticked += new TickEventHandler(UpdateCountdown);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.ReadLine();
        }

        static void UpdateCountdown(object source, TickEventArgs e)
        {
            ClearCountdownLines();

            if (Countdown.IsRunning)
            {
                Console.WriteLine(e.Countdown);
                Console.WriteLine(e.Timeprogress);
                Console.WriteLine(e.Percentprogress);

                int i = 0;
                int width = Console.WindowWidth - 4;
                double limit = Math.Floor(e.Progress * width);

                while (i < limit)
                {
                    Console.Write("#");
                    i++;
                }

                while (i < width)
                {
                    Console.Write("-");
                    i++;
                }
            }
            else
            {
                Console.Write("The countdown is over");
            }
        }

        static void ClearCountdownLines()
        {
            string blank = new string(' ', Console.WindowWidth);

            Console.CursorLeft = 0;
            Console.CursorTop -= 3;
            Console.WriteLine(blank);
            Console.WriteLine(blank);
            Console.WriteLine(blank);
            Console.Write(blank);
            Console.CursorLeft = 0;
            Console.CursorTop -= 3;
        }
    }
}
