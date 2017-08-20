using System;
using CountdownLogic;

namespace ConsoleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            int hh;
            int mm;

            try
            {
                hh = int.Parse(args[0]);
                mm = int.Parse(args[1]);
            }
            catch (Exception)
            {
                hh = 0;
                mm = 0;
            }

            DateTime starttime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            DateTime endtime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hh, mm, 0);

            if ((hh < starttime.Hour) || (hh == starttime.Hour && mm < starttime.Minute))
            {
                endtime = endtime.AddDays(1);
            }

            Countdown.TimerSetup(starttime, endtime);

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
