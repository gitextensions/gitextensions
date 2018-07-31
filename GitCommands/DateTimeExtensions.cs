// ReSharper disable once CheckNamespace

namespace System
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
        {
            if (dateTime.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime)
            {
                return DateTimeOffset.MinValue;
            }

            if (dateTime.ToUniversalTime() >= DateTimeOffset.MaxValue.UtcDateTime)
            {
                return DateTimeOffset.MaxValue;
            }

            return new DateTimeOffset(dateTime);
        }
    }
}