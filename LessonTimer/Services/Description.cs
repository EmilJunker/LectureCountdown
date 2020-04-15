using System;

namespace LessonTimer.Services
{
    public class Description
    {
        public string DescriptionString { get; private set; }
        public double CountdownLength { get; private set; }

        public Description()
        {
            Reset();
        }

        public void Reset()
        {
            DescriptionString = null;
            CountdownLength = 0;
        }

        public void Set(string description, double length)
        {
            DescriptionString = description;
            CountdownLength = length;
        }

        public static string GetDescription(string description, double length)
        {
            if (description is null)
            {
                return GetDescription(length);
            }
            return description;
        }

        public static string GetDescription(double length)
        {
            return Settings.GetCountdownDescription().Replace("#", length.ToString());
        }
    }
}
