using System;
using System.Globalization;
using System.Linq;
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
                string[] cultureDigits = CultureInfo.GetCultureInfo(culture).NumberFormat.NativeDigits;
                string result = String.Join(String.Empty, number.ToString().Select(
                    c => Char.IsDigit(c) ? cultureDigits[Int32.Parse(c.ToString())] : c.ToString()));
                return result;
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
                int result = Int32.Parse(String.Join(String.Empty, numberString.Select(
                    c => Char.GetNumericValue(c))));
                return result;
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
