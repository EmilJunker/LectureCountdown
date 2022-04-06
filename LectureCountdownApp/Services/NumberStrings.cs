using ArabicNumbersConverter;
using System;
using Windows.Globalization;

namespace LectureCountdown.Services
{
    public static class NumberStrings
    {
        public static string NumberToCultureString(int number)
        {
            try
            {
                string culture = ApplicationLanguages.PrimaryLanguageOverride;
                if (culture == String.Empty)
                {
                    culture = ApplicationLanguages.Languages[0];
                }
                return number.ToCultureString(culture);
            }
            catch (Exception)
            {
                return number.ToString();
            }
        }

        public static int NumberStringToInteger(string numberString)
        {
            try
            {
                return numberString.ToInteger();
            }
            catch (Exception)
            {
                return Int32.Parse(numberString);
            }
        }

        public static int ParseLengthString24Hours(string lengthString)
        {
            int length = NumberStringToInteger(lengthString);
            if (0 < length && length < 1440)
            {
                return length;
            }
            throw new ArgumentException();
        }
    }
}
