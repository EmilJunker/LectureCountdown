using System;
using System.Collections.Generic;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace LessonTimer.Services
{
    class Notifications
    {
        private static readonly List<string> emoji = new List<string>(new string[] { "\U0001F600", "\U0001F601", "\U0001F602", "\U0001F923", "\U0001F603", "\U0001F604", "\U0001F605", "\U0001F606",
            "\U0001F609", "\U0001F60A", "\U0001F60B", "\U0001F60E", "\U0001F60D", "\U0000263A", "\U0001F642", "\U0001F917", "\U0001F914", "\U0001F610", "\U0001F611", "\U0001F636",
            "\U0001F644", "\U0001F60F", "\U0001F623", "\U0001F625", "\U0001F62E", "\U0001F910", "\U0001F615", "\U0001F643", "\U0001F62F", "\U0001F62A", "\U0001F62B", "\U0001F634",
            "\U0001F60C", "\U0001F61B", "\U0001F61C", "\U0001F61D", "\U0001F924", "\U0001F612", "\U0001F613", "\U0001F614", "\U0001F615", "\U0001F643", "\U0001F632", "\U00002639",
            "\U0001F641", "\U0001F616", "\U0001F61E", "\U0001F61F", "\U0001F624", "\U0001F622", "\U0001F62D", "\U0001F626", "\U0001F627", "\U0001F628", "\U0001F629", "\U0001F62C",
            "\U0001F630", "\U0001F631", "\U0001F633", "\U0001F635", "\U0001F621", "\U0001F620", "\U0001F921", "\U0001F913", "\U0001F4A9", "\U0001F648", "\U0001F649", "\U0001F64A" });

        private static ScheduledToastNotification toast;

        public static void ScheduleToastNotification(bool sound, DateTime starttime, DateTime endtime)
        {
            CancelToastNotification();

            double durationTotalSeconds = endtime.Subtract(starttime).TotalSeconds;

            if (durationTotalSeconds > 0)
            {
                Random rand = new Random();
                int index = rand.Next(0, emoji.Count);

                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

                string title = loader.GetString("NotificationTitle");
                string content = loader.GetString("NotificationText1") + durationTotalSeconds.ToString() + loader.GetString("NotificationText2") + " " + emoji[index];
                string silent = sound ? "false" : "true";

                string toastXmlString =
                $@"<toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{content}</text>
                        </binding>
                    </visual>
                    <audio silent='{silent}'/>
                </toast>";

                XmlDocument toastXml = new XmlDocument();
                toastXml.LoadXml(toastXmlString);
                toast = new ScheduledToastNotification(toastXml, endtime);

                ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
            }
        }

        public static void CancelToastNotification()
        {
            try
            {
                ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(toast);
            }
            catch (Exception) { }
        }

        public static void UseToastNotification(ScheduledToastNotification toast)
        {
            Notifications.toast = toast;
        }
    }
}
