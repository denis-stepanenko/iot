using System;

namespace IOTAPI.Data;

public static class DateTimeHelper
{
    public static long DateTimeToUnixMilliseconds(DateTime date)
    {
        DateTimeOffset dto = new DateTimeOffset(date);
        return dto.ToUnixTimeMilliseconds();
    }

    public static DateTime UnixMillisecondsToDateTime(long timestamp)
    {
        var offset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        return offset.LocalDateTime;
    }
}
