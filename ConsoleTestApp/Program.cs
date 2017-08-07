using System;
using CountdownLogic;

namespace ConsoleTestApp {

    class Program {

        static Tick tick;

        static int hh;
        static int mm;

        static void Main(string[] args) {

            try {
                hh = int.Parse(args[0]);
                mm = int.Parse(args[1]);
            }
            catch (Exception) {
                hh = 0;
                mm = 0;
            }

            tick = new Tick();
            tick.Ticked += new TickEventHandler(UpdateCountdown);
            tick.Ended += new EndEventHandler(EndCountdown);

            Countdown.TimerSetup(hh, mm, tick);

            Console.ReadLine();

        }

        static void EndCountdown(object source, EventArgs e) {

            Console.Clear();

            Console.WriteLine("The countdown is over");

        }

        static void UpdateCountdown(object source, TickEventArgs e) {

            Console.Clear();

            Console.WriteLine(e.Countdown);
            Console.WriteLine(e.Timeprogress);
            Console.WriteLine(e.Percentprogress);

            int i = 0;
            double limit = Math.Floor((e.Progress * 100));

            while (i < limit) {
                Console.Write("#");
                i++;
            }

            while (i < 100) {
                Console.Write("-");
                i++;
            }

        }

    }
}
