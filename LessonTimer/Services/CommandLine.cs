using CommandLine;
using CountdownLogic;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LessonTimer.Services
{
    public class Options
    {
        [Option("length", SetName = "length")]
        public string Length { get; set; }

        [Option("end-time", SetName = "time")]
        public string EndTime { get; set; }

        [Option("description", Default = null)]
        public string Description { get; set; }

        [Option("notification", Default = null)]
        public string Notification { get; set; }

        [Option("notification-sound", Default = null)]
        public int? NotificationSound { get; set; }

        [Option("compact-mode", Default = false)]
        public bool CompactMode { get; set; }
    }

    public struct ParsedOptions
    {
        public DateTime? starttime;
        public DateTime? endtime;
        public int? length;
        public string description;
        public string notificationMode;
        public string notificationSound;
        public bool launchInCompactMode;
    }

    public static class CommandLineParser
    {
        public static ParsedOptions Parse(string arguments)
        {
            ParsedOptions options = new ParsedOptions();

            Regex r = new Regex("([^\" ][^ ]*)|(\"[^\"]*\")");
            string[] args = r.Matches(arguments).OfType<Match>().Select(m => m.Value).ToArray();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if (o.Length != null)
                    {
                        try
                        {
                            options.length = Countdown.ParseLengthString24Hours(o.Length);
                            options.starttime = Countdown.DateTimeNow();
                            options.endtime = Countdown.LengthToEndTime(options.starttime.Value, options.length.Value);
                            Settings.SetCountdownBase("length");
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException();
                        }
                    }

                    if (o.EndTime != null)
                    {
                        try
                        {
                            options.endtime = OptionStringToDateTime(o.EndTime);
                            options.starttime = Countdown.DateTimeNow();
                            options.length = Countdown.EndTimeToLength(options.starttime.Value, options.endtime.Value);
                            Settings.SetCountdownBase("time");
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException();
                        }
                    }

                    if (o.Description != null)
                    {
                        options.description = o.Description.Trim('\"');
                    }

                    if (o.Notification != null)
                    {
                        string mode = o.Notification.Trim('\"');
                        if (mode == "none" || mode == "silent" || mode == "sound" || mode == "alarm")
                        {
                            options.notificationMode = mode;
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                    }

                    if (o.NotificationSound != null)
                    {
                        int index = o.NotificationSound.Value - 1;
                        try
                        {
                            options.notificationSound = Settings.AllowedNotificationSounds[index];
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException();
                        }
                    }

                    options.launchInCompactMode = o.CompactMode;
                })
                .WithNotParsed<Options>(o =>
                {
                    throw new ArgumentException();
                });

            return options;
        }

        private static DateTime OptionStringToDateTime(string param)
        {
            string time = param.Trim('\"').ToLower();
            bool pm = time.Contains("pm");
            time = time.Replace("am", String.Empty).Replace("pm", String.Empty);

            string[] hhmm = time.Split(':', '.');
            if (hhmm.Length != 2)
            {
                throw new ArgumentException();
            }

            uint hh = Convert.ToUInt32(hhmm[0]);
            if (pm && hh != 12)
            {
                hh += 12;
            }
            else if (!pm && hh == 12)
            {
                hh = 0;
            }
            uint mm = Convert.ToUInt32(hhmm[1]);

            return Countdown.DateTimeTodayOrTomorrow((int)hh, (int)mm, 0);
        }
    }
}
