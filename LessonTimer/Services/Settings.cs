using System;
using System.Collections.Generic;
using Windows.Globalization;
using Windows.Storage;

namespace LessonTimer.Services
{
    class Settings
    {
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static string CountdownBase { get; private set; }
        public static string CountdownDescription { get; private set; }
        public static List<int> LectureLengths { get; private set; }
        public static int LectureLengthRoundTo { get; private set; }
        public static bool AcademicQuarterBeginEnabled { get; private set; }
        public static bool AcademicQuarterEndEnabled { get; private set; }
        public static bool StartTimeCarryBackEnabled { get; private set; }
        public static bool NotificationsEnabled { get; private set; }
        public static bool NotificationSoundEnabled { get; private set; }
        public static bool NotificationAlarmModeEnabled { get; private set; }
        public static string NotificationSound { get; private set; }
        public static string LanguageUI { get; private set; }
        public static string ClockFormat { get; private set; }
        public static string Theme { get; private set; }

        public static string GetCountdownDescription()
        {
            if (CountdownDescription is null)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                return loader.GetString("MinuteLecture");
            }
            return CountdownDescription;
        }

        public static void SetCountdownBase(string value)
        {
            CountdownBase = value;
            localSettings.Values["CountdownBase"] = CountdownBase;
        }

        public static void SetCountdownDescription(string value)
        {
            CountdownDescription = value;
            localSettings.Values["CountdownDescription"] = CountdownDescription;
        }

        public static void SetLectureLengths(List<int> values)
        {
            LectureLengths = values;
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
                LectureLengths = new List<int> { 45, 60, 90 };
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

        public static void SetLectureLengthRoundTo(int value)
        {
            LectureLengthRoundTo = value;
            localSettings.Values["LectureLengthRoundTo"] = LectureLengthRoundTo;
        }

        public static void SetAcademicQuarterBeginEnabled(bool value)
        {
            AcademicQuarterBeginEnabled = value;
            localSettings.Values["AcademicQuarterBeginEnabled"] = AcademicQuarterBeginEnabled;
        }

        public static void SetAcademicQuarterEndEnabled(bool value)
        {
            AcademicQuarterEndEnabled = value;
            localSettings.Values["AcademicQuarterEndEnabled"] = AcademicQuarterEndEnabled;
        }

        public static void SetStartTimeCarryBackEnabled(bool value)
        {
            StartTimeCarryBackEnabled = value;
            localSettings.Values["StartTimeCarryBackEnabled"] = StartTimeCarryBackEnabled;
        }

        public static void SetNotificationsEnabled(bool value)
        {
            NotificationsEnabled = value;
            localSettings.Values["NotificationsEnabled"] = NotificationsEnabled;
        }

        public static void SetNotificationSoundEnabled(bool value)
        {
            NotificationSoundEnabled = value;
            localSettings.Values["NotificationSoundEnabled"] = NotificationSoundEnabled;
        }

        public static void SetNotificationAlarmModeEnabled(bool value)
        {
            NotificationAlarmModeEnabled = value;
            localSettings.Values["NotificationAlarmModeEnabled"] = NotificationAlarmModeEnabled;
        }

        public static void SetNotificationSound(string value)
        {
            NotificationSound = value;
            localSettings.Values["NotificationSound"] = NotificationSound;
        }

        public static void SetLanguageUI(string value)
        {
            LanguageUI = value;
            localSettings.Values["LanguageUI"] = LanguageUI;
        }

        public static void SetClockFormat(string value)
        {
            ClockFormat = value;
            localSettings.Values["ClockFormat"] = ClockFormat;
        }

        public static void SetTheme(string value)
        {
            Theme = value;
            localSettings.Values["Theme"] = Theme;
        }

        public static void LoadSettings()
        {
            CountdownBase = localSettings.Values["CountdownBase"] as string;
            if (CountdownBase is null)
            {
                SetCountdownBase("length");
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
                SetLectureLengthRoundTo(5);
            }
            else
            {
                LectureLengthRoundTo = _LectureLengthRoundTo.Value;
            }

            bool? _AcademicQuarterBeginEnabled = localSettings.Values["AcademicQuarterBeginEnabled"] as bool?;
            if (_AcademicQuarterBeginEnabled is null)
            {
                SetAcademicQuarterBeginEnabled(false);
            }
            else
            {
                AcademicQuarterBeginEnabled = _AcademicQuarterBeginEnabled.Value;
            }

            bool? _AcademicQuarterEndEnabled = localSettings.Values["AcademicQuarterEndEnabled"] as bool?;
            if (_AcademicQuarterEndEnabled is null)
            {
                SetAcademicQuarterEndEnabled(false);
            }
            else
            {
                AcademicQuarterEndEnabled = _AcademicQuarterEndEnabled.Value;
            }

            bool? _StartTimeCarryBackEnabled = localSettings.Values["StartTimeCarryBackEnabled"] as bool?;
            if (_StartTimeCarryBackEnabled is null)
            {
                SetStartTimeCarryBackEnabled(false);
            }
            else
            {
                StartTimeCarryBackEnabled = _StartTimeCarryBackEnabled.Value;
            }

            bool? _NotificationsEnabled = localSettings.Values["NotificationsEnabled"] as bool?;
            if (_NotificationsEnabled is null)
            {
                SetNotificationsEnabled(true);
            }
            else
            {
                NotificationsEnabled = _NotificationsEnabled.Value;
            }

            bool? _NotificationSoundEnabled = localSettings.Values["NotificationSoundEnabled"] as bool?;
            if (_NotificationSoundEnabled is null)
            {
                SetNotificationSoundEnabled(false);
            }
            else
            {
                NotificationSoundEnabled = _NotificationSoundEnabled.Value;
            }

            bool? _NotificationAlarmModeEnabled = localSettings.Values["NotificationAlarmModeEnabled"] as bool?;
            if (_NotificationAlarmModeEnabled is null)
            {
                SetNotificationAlarmModeEnabled(false);
            }
            else
            {
                NotificationAlarmModeEnabled = _NotificationAlarmModeEnabled.Value;
            }

            NotificationSound = localSettings.Values["NotificationSound"] as string;
            if (NotificationSound is null)
            {
                SetNotificationSound(AllowedNotificationSounds[0]);
            }

            LanguageUI = localSettings.Values["LanguageUI"] as string;
            if (LanguageUI is null)
            {
                SetLanguageUI(String.Empty);
            }

            ClockFormat = localSettings.Values["ClockFormat"] as string;
            if (ClockFormat is null)
            {
                SetClockFormat(new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shorttime", new[] { new GeographicRegion().Code }).Clock);
            }

            Theme = localSettings.Values["Theme"] as string;
            if (Theme is null)
            {
                SetTheme(String.Empty);
            }
        }

        public static readonly List<string> AllowedNotificationSounds = new List<string> {
            "ms-winsoundevent:Notification.Default",
            "ms-winsoundevent:Notification.IM",
            "ms-winsoundevent:Notification.Mail",
            "ms-winsoundevent:Notification.Reminder",
            "ms-winsoundevent:Notification.Looping.Alarm",
            "ms-winsoundevent:Notification.Looping.Alarm2",
            "ms-winsoundevent:Notification.Looping.Alarm3",
            "ms-winsoundevent:Notification.Looping.Alarm4",
            "ms-winsoundevent:Notification.Looping.Alarm5",
            "ms-winsoundevent:Notification.Looping.Alarm6",
            "ms-winsoundevent:Notification.Looping.Alarm7",
            "ms-winsoundevent:Notification.Looping.Alarm8",
            "ms-winsoundevent:Notification.Looping.Alarm9",
            "ms-winsoundevent:Notification.Looping.Alarm10"
        };

        public static int FixNotificationSoundForAlarmMode()
        {
            if (AllowedNotificationSounds.IndexOf(NotificationSound) < 4)
            {
                SetNotificationSound(AllowedNotificationSounds[4]);
                return 4;
            }
            return -1;
        }

        public static bool FixAlarmModeForNotificationSound()
        {
            if (NotificationAlarmModeEnabled && AllowedNotificationSounds.IndexOf(NotificationSound) < 4)
            {
                SetNotificationAlarmModeEnabled(false);
                return true;
            }
            return false;
        }
    }
}
