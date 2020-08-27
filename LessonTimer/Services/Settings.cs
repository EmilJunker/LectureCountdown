using System;
using System.Collections.Generic;
using Windows.Globalization;
using Windows.Storage;

namespace LessonTimer.Services
{
    class Settings
    {
        public static string CountdownBase { get; set; }
        public static string CountdownDescription { get; set; }
        public static List<int> LectureLengths { get; set; }
        public static int LectureLengthRoundTo { get; set; }
        public static bool AcademicQuarterBeginEnabled { get; set; }
        public static bool AcademicQuarterEndEnabled { get; set; }
        public static bool NotificationsEnabled { get; set; }
        public static bool NotificationSoundEnabled { get; set; }
        public static string NotificationSound{ get; set; }
        public static string LanguageUI { get; set; }
        public static string ClockFormat { get; set; }
        public static string Theme { get; set; }

        public static string GetCountdownDescription()
        {
            if (CountdownDescription is null)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                return loader.GetString("MinuteLecture");
            }
            return CountdownDescription;
        }

        public static void LoadSettings(ApplicationDataContainer localSettings)
        {
            try
            {
                CountdownBase = (string)localSettings.Values["countdownBase"];

                if (CountdownBase is null)
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
                CountdownDescription = (string)localSettings.Values["countdownDescription"];

                if (CountdownDescription is null)
                {
                    CountdownDescription = null;
                }
            }
            catch (NullReferenceException)
            {
                CountdownDescription = null;
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
                NotificationSound = (string)localSettings.Values["notificationSound"];

                if (NotificationSound is null)
                {
                    NotificationSound = "ms-winsoundevent:Notification.Default";
                }
            }
            catch (NullReferenceException)
            {
                NotificationSound = "ms-winsoundevent:Notification.Default";
            }

            try
            {
                LanguageUI = (string)localSettings.Values["languageUI"];

                if (LanguageUI is null)
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

                if (ClockFormat is null)
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

                if (Theme is null)
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
