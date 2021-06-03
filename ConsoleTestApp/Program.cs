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

            try
            {
                arg0 = int.Parse(args[0]);
                arg1 = int.Parse(args[1]);
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
            else
            {
                // have arg0 arg1 -> use as endtime today or tomorrow
                DateTime endtime = Countdown.DateTimeTodayOrTomorrow((int)arg0, (int)arg1, 0);
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
