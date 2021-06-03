using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Appointments;

namespace LessonTimer.Services
{
    class TimeSuggestions
    {
        public static int SuggestionsIterator { get; set; } = 0;
        public static int CalendarSuggestionsIterator { get; set; } = 0;

        public static (string lengthString, int length) GetLengthSuggestion()
        {
            int length = Settings.LectureLengths[SuggestionsIterator];

            SuggestionsIterator++;
            if (SuggestionsIterator >= Settings.LectureLengths.Count)
            {
                SuggestionsIterator = 0;
            }

            return (length.ToString(), length);
        }

        public static (TimeSpan endtimePick, int length) GetEndTimeSuggestion()
        {
            int length = Settings.LectureLengths[SuggestionsIterator];

            DateTime time = DateTime.Now.AddMinutes(length);
            TimeSpan span = TimeSpan.FromMinutes(Settings.LectureLengthRoundTo);

            SuggestionsIterator++;
            if (SuggestionsIterator >= Settings.LectureLengths.Count)
            {
                SuggestionsIterator = 0;
            }

            TimeSpan t = time.Subtract(DateTime.MinValue).Add(new TimeSpan(0, span.Minutes / 2, 0));
            DateTime newTime = DateTime.MinValue.Add(new TimeSpan(0, ((int)t.TotalMinutes) / (int)span.TotalMinutes * span.Minutes, 0));

            return (new TimeSpan(newTime.Hour, newTime.Minute, 0), length);
        }

        public static (DateTime? starttime, string lengthString, string description) GetLengthSuggestion(IReadOnlyList<Appointment> allAppointments)
        {
            (String subject, DateTime start, DateTime end) nextAppointment;
            try
            {
                nextAppointment = GetNextAppointment(allAppointments);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (null, null, null);
            }

            int appointmentLength = (int)nextAppointment.end.Subtract(nextAppointment.start).TotalMinutes;

            if (Settings.StartTimeCarryBackEnabled)
            {
                return (nextAppointment.start, appointmentLength.ToString(), nextAppointment.subject);
            }
            return (null, appointmentLength.ToString(), nextAppointment.subject);
        }

        public static (DateTime? starttime, TimeSpan endtimePick, string description) GetEndTimeSuggestion(IReadOnlyList<Appointment> allAppointments)
        {
            (String subject, DateTime start, DateTime end) nextAppointment;
            try
            {
                nextAppointment = GetNextAppointment(allAppointments);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (null, new TimeSpan(), null);
            }

            TimeSpan endtimePick = new TimeSpan(nextAppointment.end.Hour, nextAppointment.end.Minute, 0);

            if (Settings.StartTimeCarryBackEnabled)
            {
                return (nextAppointment.start, endtimePick, nextAppointment.subject);
            }
            return (null, endtimePick, nextAppointment.subject);
        }

        private static (String, DateTime, DateTime) GetNextAppointment(IReadOnlyList<Appointment> allAppointments)
        {
            var appointments = new List<(String, DateTime, DateTime)>();

            foreach (Appointment a in allAppointments)
            {
                if (!a.AllDay)
                {
                    int addMinutes = 0;
                    int subtractMinutes = 0;
                    if (a.Duration > TimeSpan.FromMinutes(30))
                    {
                        if (Settings.AcademicQuarterBeginEnabled)
                        {
                            addMinutes = 15;
                        }
                        if (Settings.AcademicQuarterEndEnabled)
                        {
                            subtractMinutes = -15;
                        }
                    }
                    DateTime start = a.StartTime.AddMinutes(addMinutes).DateTime;
                    DateTime end = a.StartTime.Add(a.Duration).AddMinutes(subtractMinutes).DateTime;

                    if (end > DateTime.Now)
                    {
                        appointments.Add((a.Subject, start, end));
                    }
                }
            }

            (String, DateTime, DateTime) nextAppointment;
            try
            {
                nextAppointment = appointments[CalendarSuggestionsIterator];
            }
            catch (ArgumentOutOfRangeException)
            {
                CalendarSuggestionsIterator = 0;
                try
                {
                    nextAppointment = appointments[CalendarSuggestionsIterator];
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw;
                }
            }

            CalendarSuggestionsIterator++;

            return nextAppointment;
        }
    }
}
