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
        public static bool NotificationAlarmModeEnabled { get; set; }
        public static string NotificationSound { get; set; }
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

        public static void ResetLectureLengths(ApplicationDataContainer localSettings, int i)
        {
            if (i == 0)
            {
                LectureLengths = new List<int>(new int[] { 45, 60, 90 });
                localSettings.Values["lectureLengths0"] = 45; // remove
                localSettings.Values["lectureLengths1"] = 60; // remove
                localSettings.Values["lectureLengths2"] = 90; // remove
                localSettings.Values["LectureLengths0"] = 45;
                localSettings.Values["LectureLengths1"] = 60;
                localSettings.Values["LectureLengths2"] = 90;
                i = 3;
            }
            while (i <= 7)
            {
                localSettings.Values[String.Format("lectureLengths{0}", i)] = null; // remove
                localSettings.Values[String.Format("LectureLengths{0}", i)] = null;
                i++;
            }
        }

        public static void LoadSettings(ApplicationDataContainer localSettings)
        {
            CountdownBase = localSettings.Values["countdownBase"] as string; // change
            if (CountdownBase is null)
            {
                CountdownBase = "length";
                localSettings.Values["CountdownBase"] = CountdownBase;
            }
            else // remove
            {
                localSettings.Values["CountdownBase"] = CountdownBase;
            }

            CountdownDescription = localSettings.Values["countdownDescription"] as string; // change
            if (CountdownDescription is null)
            {
                CountdownDescription = null;
                localSettings.Values["CountdownDescription"] = CountdownDescription;
            }
            else // remove
            {
                localSettings.Values["CountdownDescription"] = CountdownDescription;
            }

            try
            {
                LectureLengths = new List<int> { (int)localSettings.Values["lectureLengths0"] }; // change
                localSettings.Values["LectureLengths0"] = (int)localSettings.Values["lectureLengths0"]; // remove
                try
                {
                    LectureLengths.Add((int)localSettings.Values["lectureLengths1"]); // change
                    localSettings.Values["LectureLengths1"] = (int)localSettings.Values["lectureLengths1"]; // remove
                    LectureLengths.Add((int)localSettings.Values["lectureLengths2"]); // change
                    localSettings.Values["LectureLengths2"] = (int)localSettings.Values["lectureLengths2"]; // remove
                    LectureLengths.Add((int)localSettings.Values["lectureLengths3"]); // change
                    localSettings.Values["LectureLengths3"] = (int)localSettings.Values["lectureLengths3"]; // remove
                    LectureLengths.Add((int)localSettings.Values["lectureLengths4"]); // change
                    localSettings.Values["LectureLengths4"] = (int)localSettings.Values["lectureLengths4"]; // remove
                    LectureLengths.Add((int)localSettings.Values["lectureLengths5"]); // change
                    localSettings.Values["LectureLengths5"] = (int)localSettings.Values["lectureLengths5"]; // remove
                    LectureLengths.Add((int)localSettings.Values["lectureLengths6"]); // change
                    localSettings.Values["LectureLengths6"] = (int)localSettings.Values["lectureLengths6"]; // remove
                    LectureLengths.Add((int)localSettings.Values["lectureLengths7"]); // change
                    localSettings.Values["LectureLengths7"] = (int)localSettings.Values["lectureLengths7"]; // remove
                }
                catch (NullReferenceException) { }
            }
            catch (NullReferenceException)
            {
                ResetLectureLengths(localSettings, 0);
            }

            int? _LectureLengthRoundTo = localSettings.Values["lectureLengthRoundTo"] as int?; // change
            if (_LectureLengthRoundTo is null)
            {
                LectureLengthRoundTo = 5;
                localSettings.Values["LectureLengthRoundTo"] = LectureLengthRoundTo;
            }
            else
            {
                LectureLengthRoundTo = (int)_LectureLengthRoundTo;
                localSettings.Values["LectureLengthRoundTo"] = LectureLengthRoundTo; // remove
            }

            bool? _AcademicQuarterBeginEnabled = localSettings.Values["academicQuarterBeginEnabled"] as bool?; // change
            if (_AcademicQuarterBeginEnabled is null)
            {
                AcademicQuarterBeginEnabled = false;
                localSettings.Values["AcademicQuarterBeginEnabled"] = AcademicQuarterBeginEnabled;
            }
            else
            {
                AcademicQuarterBeginEnabled = (bool)_AcademicQuarterBeginEnabled;
                localSettings.Values["AcademicQuarterBeginEnabled"] = AcademicQuarterBeginEnabled; // remove
            }

            bool? _AcademicQuarterEndEnabled = localSettings.Values["academicQuarterEndEnabled"] as bool?; // change
            if (_AcademicQuarterEndEnabled is null)
            {
                AcademicQuarterEndEnabled = false;
                localSettings.Values["AcademicQuarterEndEnabled"] = AcademicQuarterEndEnabled;
            }
            else
            {
                AcademicQuarterEndEnabled = (bool)_AcademicQuarterEndEnabled;
                localSettings.Values["AcademicQuarterEndEnabled"] = AcademicQuarterEndEnabled; // remove
            }

            bool? _NotificationsEnabled = localSettings.Values["notificationsEnabled"] as bool?; // change
            if (_NotificationsEnabled is null)
            {
                NotificationsEnabled = true;
                localSettings.Values["NotificationsEnabled"] = NotificationsEnabled;
            }
            else
            {
                NotificationsEnabled = (bool)_NotificationsEnabled;
                localSettings.Values["NotificationsEnabled"] = NotificationsEnabled; // remove
            }

            bool? _NotificationSoundEnabled = localSettings.Values["notificationSoundEnabled"] as bool?; // change
            if (_NotificationSoundEnabled is null)
            {
                NotificationSoundEnabled = false;
                localSettings.Values["NotificationSoundEnabled"] = NotificationSoundEnabled;
            }
            else
            {
                NotificationSoundEnabled = (bool)_NotificationSoundEnabled;
                localSettings.Values["NotificationSoundEnabled"] = NotificationSoundEnabled; // remove
            }

            bool? _NotificationAlarmModeEnabled = localSettings.Values["notificationAlarmModeEnabled"] as bool?; // change
            if (_NotificationAlarmModeEnabled is null)
            {
                NotificationAlarmModeEnabled = false;
                localSettings.Values["NotificationAlarmModeEnabled"] = NotificationAlarmModeEnabled;
            }
            else
            {
                NotificationAlarmModeEnabled = (bool)_NotificationAlarmModeEnabled;
                localSettings.Values["NotificationAlarmModeEnabled"] = NotificationAlarmModeEnabled; // remove
            }

            NotificationSound = localSettings.Values["notificationSound"] as string; // change
            if (NotificationSound is null)
            {
                NotificationSound = "ms-winsoundevent:Notification.Default";
                localSettings.Values["NotificationSound"] = NotificationSound;
            }
            else // remove
            {
                localSettings.Values["NotificationSound"] = NotificationSound;
            }

            LanguageUI = localSettings.Values["languageUI"] as string; // change
            if (LanguageUI is null)
            {
                LanguageUI = String.Empty;
                localSettings.Values["LanguageUI"] = LanguageUI;
            }
            else // remove
            {
                localSettings.Values["LanguageUI"] = LanguageUI;
            }

            ClockFormat = localSettings.Values["clockFormat"] as string; // change
            if (ClockFormat is null)
            {
                ClockFormat = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code }).Clock;
                localSettings.Values["ClockFormat"] = ClockFormat;
            }
            else // remove
            {
                localSettings.Values["ClockFormat"] = ClockFormat;
            }

            Theme = localSettings.Values["theme"] as string; // change
            if (Theme is null)
            {
                Theme = String.Empty;
                localSettings.Values["Theme"] = Theme;
            }
            else // remove
            {
                localSettings.Values["Theme"] = Theme;
            }
        }
    }
}
