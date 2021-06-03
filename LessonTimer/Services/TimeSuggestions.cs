using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Appointments;

namespace LessonTimer.Services
{
    class TimeSuggestions
    {
        public static int SuggestionsIterator { get; set; } = 0;
        public static int CalendarSuggestionsIterator { get; set; } = 0;

        public static (string lengthString, double length) GetLengthSuggestion()
        {
            double length = Settings.LectureLengths[SuggestionsIterator];

            SuggestionsIterator++;
            if (SuggestionsIterator >= Settings.LectureLengths.Count)
            {
                SuggestionsIterator = 0;
            }

            return (length.ToString(), length);
        }

        public static (TimeSpan endtime, double length) GetEndTimeSuggestion()
        {
            double length = Settings.LectureLengths[SuggestionsIterator];

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

        public static (string lengthString, string description) GetLengthSuggestion(IReadOnlyList<Appointment> allAppointments)
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
                    return (null, null);
                }
            }

            CalendarSuggestionsIterator++;

            double nextAppointmentLength = nextAppointment.Duration.TotalMinutes;

            if (Settings.AcademicQuarterBeginEnabled && Settings.AcademicQuarterEndEnabled)
            {
                if (nextAppointmentLength > 30)
                {
                    nextAppointmentLength -= 30;
                }
            }
            else if (Settings.AcademicQuarterBeginEnabled || Settings.AcademicQuarterEndEnabled)
            {
                if (nextAppointmentLength > 15)
                {
                    nextAppointmentLength -= 15;
                }
            }

            return (nextAppointmentLength.ToString(), nextAppointment.Subject);
        }

        public static (TimeSpan endtime, string description) GetEndTimeSuggestion(IReadOnlyList<Appointment> allAppointments)
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
                    return (new TimeSpan(), null);
                }
            }

            CalendarSuggestionsIterator++;

            DateTimeOffset nextAppointmentEndTime = nextAppointment.StartTime.Add(nextAppointment.Duration);

            if (Settings.AcademicQuarterEndEnabled && nextAppointment.Duration > TimeSpan.FromMinutes(15))
            {
                nextAppointmentEndTime = nextAppointmentEndTime.AddMinutes(-15);
            }

            return (new TimeSpan(nextAppointmentEndTime.Hour, nextAppointmentEndTime.Minute, 0), nextAppointment.Subject);
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
