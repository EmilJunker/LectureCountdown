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
            Appointment nextAppointment;
            try
            {
                nextAppointment = GetNextAppointment(allAppointments);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (null, null, null);
            }

            DateTime nextAppointmentStarttime = nextAppointment.StartTime.DateTime;
            int nextAppointmentLength = (int)nextAppointment.Duration.TotalMinutes;

            if (nextAppointmentLength > 30)
            {
                if (Settings.AcademicQuarterBeginEnabled)
                {
                    if (Settings.StartTimeCarryBackEnabled)
                    {
                        nextAppointmentStarttime = nextAppointmentStarttime.AddMinutes(15);
                    }
                    nextAppointmentLength -= 15;
                }
                if (Settings.AcademicQuarterEndEnabled)
                {
                    nextAppointmentLength -= 15;
                }
            }

            if (Settings.StartTimeCarryBackEnabled)
            {
                return (nextAppointmentStarttime, nextAppointmentLength.ToString(), nextAppointment.Subject);
            }
            return (null, nextAppointmentLength.ToString(), nextAppointment.Subject);
        }

        public static (DateTime? starttime, TimeSpan endtimePick, string description) GetEndTimeSuggestion(IReadOnlyList<Appointment> allAppointments)
        {
            Appointment nextAppointment;
            try
            {
                nextAppointment = GetNextAppointment(allAppointments);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (null, new TimeSpan(), null);
            }

            DateTime nextAppointmentStarttime = nextAppointment.StartTime.DateTime;
            DateTime nextAppointmentEndTime = nextAppointment.StartTime.Add(nextAppointment.Duration).DateTime;

            if (nextAppointment.Duration > TimeSpan.FromMinutes(30))
            {
                if (Settings.AcademicQuarterBeginEnabled)
                {
                    if (Settings.StartTimeCarryBackEnabled)
                    {
                        nextAppointmentStarttime = nextAppointmentStarttime.AddMinutes(15);
                    }
                }
                if (Settings.AcademicQuarterEndEnabled)
                {
                    nextAppointmentEndTime = nextAppointmentEndTime.AddMinutes(-15);
                }
            }

            TimeSpan endtimePick = new TimeSpan(nextAppointmentEndTime.Hour, nextAppointmentEndTime.Minute, 0);

            if (Settings.StartTimeCarryBackEnabled)
            {
                return (nextAppointmentStarttime, endtimePick, nextAppointment.Subject);
            }
            return (null, endtimePick, nextAppointment.Subject);
        }

        private static Appointment GetNextAppointment(IReadOnlyList<Appointment> allAppointments)
        {
            List<Appointment> appointments = new List<Appointment>();

            foreach (Appointment a in allAppointments)
            {
                if (!a.AllDay)
                {
                    appointments.Add(a);
                }
            }

            Appointment nextAppointment;
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
