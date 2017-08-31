using System;

namespace Televic.Mycro.Web
{
    public static class EnumExtensions
    {
        public static string GetName<T>(this T value) where T : struct
        {
            return Enum.GetName(typeof(T), value).ToLower();
        }

        public static T Parse<T>(this string value, T defaultValue) where T : struct
        {
            T parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : defaultValue;
        }
    }
}
