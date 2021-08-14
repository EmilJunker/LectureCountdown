using System;

namespace LessonTimer.Services
{
    public static class NumberStrings
    {
        public static int ParseLengthString24Hours(string lengthString)
        {
            int length = Convert.ToInt32(lengthString);
            if (0 < length && length < 1440)
            {
                return length;
            }
            throw new ArgumentException();
        }
    }
}
