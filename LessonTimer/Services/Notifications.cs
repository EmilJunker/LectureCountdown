using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace LessonTimer.Services
{
    class Notifications
    {
        private static readonly List<string> emoji = new List<string> {
            "\U0001F600", "\U0001F603", "\U0001F604", "\U0001F601", "\U0001F606", "\U0001F605", "\U0001F923", "\U0001F602", "\U0001F642", "\U0001F643",
            "\U0001F609", "\U0001F60A", "\U0001F60D", "\U0001F929", "\U0000263A", "\U0001F60B", "\U0001F61B", "\U0001F61C", "\U0001F92A", "\U0001F61D",
            "\U0001F917", "\U0001F92D", "\U0001F92B", "\U0001F914", "\U0001F910", "\U0001F928", "\U0001F610", "\U0001F611", "\U0001F636", "\U0001F60F",
            "\U0001F612", "\U0001F644", "\U0001F62C", "\U0001F60C", "\U0001F614", "\U0001F62A", "\U0001F924", "\U0001F634", "\U0001F974", "\U0001F635",
            "\U0001F92F", "\U0001F920", "\U0001F973", "\U0001F60E", "\U0001F913", "\U0001F9D0", "\U0001F615", "\U0001F61F", "\U0001F641", "\U00002639",
            "\U0001F62E", "\U0001F62F", "\U0001F632", "\U0001F633", "\U0001F97A", "\U0001F626", "\U0001F627", "\U0001F628", "\U0001F631", "\U0001F616",
            "\U0001F623", "\U0001F61E", "\U0001F613", "\U0001F629", "\U0001F62B", "\U0001F971", "\U0001F921", "\U0001F63A", "\U0001F638", "\U0001F639",
            "\U0001F63B", "\U0001F63C", "\U0001F63D", "\U0001F640", "\U0001F63F", "\U0001F63E", "\U0001F648", "\U0001F649", "\U0001F64A" };

        private static ScheduledToastNotification toast;

        public static void ScheduleToastNotification(bool sound, bool alarm, string source, DateTime starttime, DateTime endtime)
        {
            int durationTotalSeconds = (int)endtime.Subtract(starttime).TotalSeconds;

            Random rand = new Random();
            int index = rand.Next(0, emoji.Count);

            ResourceLoader loader = new ResourceLoader();

            string title = loader.GetString("NotificationTitle");
            string content = loader.GetString("NotificationText1") + durationTotalSeconds.ToString() + loader.GetString("NotificationText2") + " " + emoji[index];

            ToastContentBuilder builder = new ToastContentBuilder()
                .SetToastScenario(alarm ? ToastScenario.Alarm : ToastScenario.Default)
                .AddText(title)
                .AddText(content)
                .AddButton(new ToastButtonDismiss())
                .AddAudio(new Uri(source), alarm, !sound);

            XmlDocument toastXml = builder.GetToastContent().GetXml();

            CancelToastNotification();
            try
            {
                toast = new ScheduledToastNotification(toastXml, endtime);
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
            }
            catch (ArgumentException) { }
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
