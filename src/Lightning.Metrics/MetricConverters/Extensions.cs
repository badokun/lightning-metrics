using System;

namespace Lightning.Metrics.MetricConverters
{
    public static class Extensions
    {
        public const int TagSize = 5;

        public static long ToLong(this string value)
        {
            return value != null ? long.Parse(value) : 0;
        }

        public static int ToInt(this bool? value)
        {
            if (value.HasValue)
            {
                return Convert.ToInt32(value.Value);
            }

            return 0;
        }

        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                    ? value
                    : value.Substring(0, maxLength)
                );
        }
    }
}