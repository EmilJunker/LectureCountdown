using System;
using System.Collections.Generic;
using Windows.Globalization;
using Windows.Storage;

namespace LessonTimer.Services
{
    class Settings
    {
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static string CountdownBase { get; set; }
        public static string CountdownDescription { get; set; }
        public static List<int> LectureLengths { get; set; }
        public static int LectureLengthRoundTo { get; set; }
        public static bool AcademicQuarterBeginEnabled { get; set; }
        public static bool AcademicQuarterEndEnabled { get; set; }
        public static bool StartTimeCarryBackEnabled { get; set; }
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

        public static void SetCountdownBase()
        {
            localSettings.Values["CountdownBase"] = CountdownBase;
        }

        public static void SetCountdownDescription()
        {
            localSettings.Values["CountdownDescription"] = CountdownDescription;
        }

        public static void SetLectureLengths()
        {
            int i = 0;
            foreach (int length in LectureLengths)
            {
                localSettings.Values[String.Format("LectureLengths{0}", i)] = length;
                i++;
            }
            ResetLectureLengths(i);
        }

        public static void ResetLectureLengths(int i)
        {
            if (i == 0)
            {
                LectureLengths = new List<int>(new int[] { 45, 60, 90 });
                localSettings.Values["LectureLengths0"] = 45;
                localSettings.Values["LectureLengths1"] = 60;
                localSettings.Values["LectureLengths2"] = 90;
                i = 3;
            }
            while (i <= 7)
            {
                localSettings.Values[String.Format("LectureLengths{0}", i)] = null;
                i++;
            }
        }

        public static void SetLectureLengthRoundTo()
        {
            localSettings.Values["LectureLengthRoundTo"] = LectureLengthRoundTo;
        }

        public static void SetAcademicQuarterBeginEnabled()
        {
            localSettings.Values["AcademicQuarterBeginEnabled"] = AcademicQuarterBeginEnabled;
        }

        public static void SetAcademicQuarterEndEnabled()
        {
            localSettings.Values["AcademicQuarterEndEnabled"] = AcademicQuarterEndEnabled;
        }

        public static void SetStartTimeCarryBackEnabled()
        {
            localSettings.Values["StartTimeCarryBackEnabled"] = StartTimeCarryBackEnabled;
        }

        public static void SetNotificationsEnabled()
        {
            localSettings.Values["NotificationsEnabled"] = NotificationsEnabled;
        }

        public static void SetNotificationSoundEnabled()
        {
            localSettings.Values["NotificationSoundEnabled"] = NotificationSoundEnabled;
        }

        public static void SetNotificationAlarmModeEnabled()
        {
            localSettings.Values["NotificationAlarmModeEnabled"] = NotificationAlarmModeEnabled;
        }

        public static void SetNotificationSound()
        {
            localSettings.Values["NotificationSound"] = NotificationSound;
        }

        public static void SetLanguageUI()
        {
            localSettings.Values["LanguageUI"] = LanguageUI;
        }

        public static void SetClockFormat()
        {
            localSettings.Values["ClockFormat"] = ClockFormat;
        }

        public static void SetTheme()
        {
            localSettings.Values["Theme"] = Theme;
        }

        public static void LoadSettings()
        {
            CountdownBase = localSettings.Values["CountdownBase"] as string;
            if (CountdownBase is null)
            {
                CountdownBase = "length";
                localSettings.Values["CountdownBase"] = CountdownBase;
            }

            CountdownDescription = localSettings.Values["CountdownDescription"] as string;

            try
            {
                LectureLengths = new List<int> { (int)localSettings.Values["LectureLengths0"] };
                try
                {
                    LectureLengths.Add((int)localSettings.Values["LectureLengths1"]);
                    LectureLengths.Add((int)localSettings.Values["LectureLengths2"]);
                    LectureLengths.Add((int)localSettings.Values["LectureLengths3"]);
                    LectureLengths.Add((int)localSettings.Values["LectureLengths4"]);
                    LectureLengths.Add((int)localSettings.Values["LectureLengths5"]);
                    LectureLengths.Add((int)localSettings.Values["LectureLengths6"]);
                    LectureLengths.Add((int)localSettings.Values["LectureLengths7"]);
                }
                catch (NullReferenceException) { }
            }
            catch (NullReferenceException)
            {
                ResetLectureLengths(0);
            }

            int? _LectureLengthRoundTo = localSettings.Values["LectureLengthRoundTo"] as int?;
            if (_LectureLengthRoundTo is null)
            {
                LectureLengthRoundTo = 5;
                localSettings.Values["LectureLengthRoundTo"] = LectureLengthRoundTo;
            }
            else
            {
                LectureLengthRoundTo = _LectureLengthRoundTo.Value;
            }

            bool? _AcademicQuarterBeginEnabled = localSettings.Values["AcademicQuarterBeginEnabled"] as bool?;
            if (_AcademicQuarterBeginEnabled is null)
            {
                AcademicQuarterBeginEnabled = false;
                localSettings.Values["AcademicQuarterBeginEnabled"] = AcademicQuarterBeginEnabled;
            }
            else
            {
                AcademicQuarterBeginEnabled = _AcademicQuarterBeginEnabled.Value;
            }

            bool? _AcademicQuarterEndEnabled = localSettings.Values["AcademicQuarterEndEnabled"] as bool?;
            if (_AcademicQuarterEndEnabled is null)
            {
                AcademicQuarterEndEnabled = false;
                localSettings.Values["AcademicQuarterEndEnabled"] = AcademicQuarterEndEnabled;
            }
            else
            {
                AcademicQuarterEndEnabled = _AcademicQuarterEndEnabled.Value;
            }

            bool? _StartTimeCarryBackEnabled = localSettings.Values["StartTimeCarryBackEnabled"] as bool?;
            if (_StartTimeCarryBackEnabled is null)
            {
                StartTimeCarryBackEnabled = false;
                localSettings.Values["StartTimeCarryBackEnabled"] = StartTimeCarryBackEnabled;
            }
            else
            {
                StartTimeCarryBackEnabled = _StartTimeCarryBackEnabled.Value;
            }

            bool? _NotificationsEnabled = localSettings.Values["NotificationsEnabled"] as bool?;
            if (_NotificationsEnabled is null)
            {
                NotificationsEnabled = true;
                localSettings.Values["NotificationsEnabled"] = NotificationsEnabled;
            }
            else
            {
                NotificationsEnabled = _NotificationsEnabled.Value;
            }

            bool? _NotificationSoundEnabled = localSettings.Values["NotificationSoundEnabled"] as bool?;
            if (_NotificationSoundEnabled is null)
            {
                NotificationSoundEnabled = false;
                localSettings.Values["NotificationSoundEnabled"] = NotificationSoundEnabled;
            }
            else
            {
                NotificationSoundEnabled = _NotificationSoundEnabled.Value;
            }

            bool? _NotificationAlarmModeEnabled = localSettings.Values["NotificationAlarmModeEnabled"] as bool?;
            if (_NotificationAlarmModeEnabled is null)
            {
                NotificationAlarmModeEnabled = false;
                localSettings.Values["NotificationAlarmModeEnabled"] = NotificationAlarmModeEnabled;
            }
            else
            {
                NotificationAlarmModeEnabled = _NotificationAlarmModeEnabled.Value;
            }

            NotificationSound = localSettings.Values["NotificationSound"] as string;
            if (NotificationSound is null)
            {
                NotificationSound = "ms-winsoundevent:Notification.Default";
                localSettings.Values["NotificationSound"] = NotificationSound;
            }

            LanguageUI = localSettings.Values["LanguageUI"] as string;
            if (LanguageUI is null)
            {
                LanguageUI = String.Empty;
                localSettings.Values["LanguageUI"] = LanguageUI;
            }

            ClockFormat = localSettings.Values["ClockFormat"] as string;
            if (ClockFormat is null)
            {
                ClockFormat = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code }).Clock;
                localSettings.Values["ClockFormat"] = ClockFormat;
            }

            Theme = localSettings.Values["Theme"] as string;
            if (Theme is null)
            {
                Theme = String.Empty;
                localSettings.Values["Theme"] = Theme;
            }
        }
    }
}
