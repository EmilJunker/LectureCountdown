using System;
using System.Collections.Generic;
using Windows.Globalization;
using Windows.Storage;

namespace LessonTimer.Services
{
    class Settings
    {
        public static string CountdownBase { get; set; }
        public static List<int> LectureLengths { get; set; }
        public static int LectureLengthRoundTo { get; set; }
        public static bool AcademicQuarterBeginEnabled { get; set; }
        public static bool AcademicQuarterEndEnabled { get; set; }
        public static bool NotificationsEnabled { get; set; }
        public static bool NotificationSoundEnabled { get; set; }
        public static string LanguageUI { get; set; }
        public static string ClockFormat { get; set; }
        public static string Theme { get; set; }

        public static void LoadSettings(ApplicationDataContainer localSettings)
        {
            try
            {
                CountdownBase = (string)localSettings.Values["countdownBase"];

                if (CountdownBase == null)
                {
                    CountdownBase = "time";
                }
            }
            catch (NullReferenceException)
            {
                CountdownBase = "time";
            }

            try
            {
                LectureLengths = new List<int> { (int)localSettings.Values["lectureLengths0"] };
                try
                {
                    LectureLengths.Add((int)localSettings.Values["lectureLengths1"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths2"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths3"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths4"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths5"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths6"]);
                    LectureLengths.Add((int)localSettings.Values["lectureLengths7"]);
                }
                catch (NullReferenceException) { }
            }
            catch (NullReferenceException)
            {
                LectureLengths = new List<int>(new int[] { 45, 60, 90 });
            }

            try
            {
                LectureLengthRoundTo = (int)localSettings.Values["lectureLengthRoundTo"];
            }
            catch (NullReferenceException)
            {
                LectureLengthRoundTo = 5;
            }

            try
            {
                AcademicQuarterBeginEnabled = (bool)localSettings.Values["academicQuarterBeginEnabled"];
            }
            catch (NullReferenceException)
            {
                AcademicQuarterBeginEnabled = false;
            }

            try
            {
                AcademicQuarterEndEnabled = (bool)localSettings.Values["academicQuarterEndEnabled"];
            }
            catch (NullReferenceException)
            {
                AcademicQuarterEndEnabled = false;
            }

            try
            {
                NotificationsEnabled = (bool)localSettings.Values["notificationsEnabled"];
            }
            catch (NullReferenceException)
            {
                NotificationsEnabled = true;
            }

            try
            {
                NotificationSoundEnabled = (bool)localSettings.Values["notificationSoundEnabled"];
            }
            catch (NullReferenceException)
            {
                NotificationSoundEnabled = false;
            }

            try
            {
                LanguageUI = (string)localSettings.Values["languageUI"];

                if (LanguageUI == null)
                {
                    LanguageUI = String.Empty;
                }
            }
            catch (NullReferenceException)
            {
                LanguageUI = String.Empty;
            }

            try
            {
                ClockFormat = (string)localSettings.Values["clockFormat"];

                if (ClockFormat == null)
                {
                    ClockFormat = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code }).Clock;
                }
            }
            catch (NullReferenceException)
            {
                ClockFormat = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code }).Clock;
            }

            try
            {
                Theme = (string)localSettings.Values["theme"];

                if (Theme == null)
                {
                    Theme = String.Empty;
                }
            }
            catch (NullReferenceException)
            {
                Theme = String.Empty;
            }
        }
    }
}
