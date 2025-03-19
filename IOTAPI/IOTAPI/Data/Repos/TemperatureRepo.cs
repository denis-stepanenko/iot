using IOTAPI.Models;
using NRedisTimeSeries;
using NRedisTimeSeries.Commands.Enums;
using NRedisTimeSeries.DataTypes;
using StackExchange.Redis;

namespace IOTAPI.Data.Repos;

public class TemperatureRepo
{
    private readonly IDatabase _db;

    public TemperatureRepo(RedisContext context)
    {
        _db = context.db;
    }

    public async Task CreateSeriesIfDosentExistAsync(string name)
    {
        if (!await _db.KeyExistsAsync(name))
        {
            _db.TimeSeriesCreate(
                name,
                retentionTime: 0,
                labels: new List<TimeSeriesLabel> { },
                duplicatePolicy: TsDuplicatePolicy.MAX);
        }
    }

    public async Task DeleteSeries(string name)
    {
        if (!await _db.KeyExistsAsync(name))
            return;

        await _db.KeyDeleteAsync(name);
    }

    public async Task<Temperature?> GetLastAsync(string deviceName)
    {
        if (!await _db.KeyExistsAsync(deviceName))
            return null;

        // Get sample with the highest timestamp
        TimeSeriesTuple result = await _db.TimeSeriesGetAsync(deviceName);

        if (result == null)
            return null;

        return new Temperature
        {
            Date = UnixMillisecondsToDateTime(result.Time),
            DeviceName = deviceName,
            Value = result.Val
        };
    }

    public async Task<List<Temperature>> GetLastItemsAsync(string deviceName, int count)
    {
        if (!await _db.KeyExistsAsync(deviceName))
            return new List<Temperature>();

        // Get first N samples from reverse range from lowest to highest timestamp 
        IReadOnlyList<TimeSeriesTuple> result = await _db.TimeSeriesRevRangeAsync(deviceName, "-", "+", count: count);

        var items = result.Select(x => new Temperature
        {
            Date = UnixMillisecondsToDateTime(x.Time),
            DeviceName = deviceName,
            Value = x.Val
        }).ToList();

        return items;
    }

    public async Task<List<Temperature>> GetItemsByPeriod(string deviceName, DateTime start, DateTime end)
    {
        if (!await _db.KeyExistsAsync(deviceName))
            return new List<Temperature>();

        long startTimestamp = DateTimeToUnixMilliseconds(start);
        long endTimeStamp = DateTimeToUnixMilliseconds(end);

        IReadOnlyList<TimeSeriesTuple> result = await _db.TimeSeriesRevRangeAsync(deviceName, startTimestamp, endTimeStamp);

        var items = result.Select(x => new Temperature
        {
            Date = UnixMillisecondsToDateTime(x.Time),
            DeviceName = deviceName,
            Value = x.Val
        }).ToList();

        return items;
    }

    public async Task<List<Temperature>> GetAverageByDuration(string deviceName, DateTime start, DateTime end, long duration)
    {
        if (!await _db.KeyExistsAsync(deviceName))
            return new List<Temperature>();

        long startTimestamp = DateTimeToUnixMilliseconds(start);
        long endTimeStamp = DateTimeToUnixMilliseconds(end);

        IReadOnlyList<TimeSeriesTuple> result = await _db.TimeSeriesRangeAsync(
            deviceName,
            startTimestamp,
            endTimeStamp,
            aggregation: TsAggregation.Avg,
            timeBucket: duration);

        var items = result.Select(x => new Temperature
        {
            Date = UnixMillisecondsToDateTime(x.Time),
            DeviceName = deviceName,
            Value = x.Val
        }).ToList();

        return items;
    }

    public async Task AddAsync(string deviceName, double value)
    {
        await _db.TimeSeriesAddAsync(deviceName, DateTimeToUnixMilliseconds(DateTime.Now), value);
    }

    private long DateTimeToUnixMilliseconds(DateTime date)
    {
        DateTimeOffset dto = new DateTimeOffset(date);
        return dto.ToUnixTimeMilliseconds();
    }

    private DateTime UnixMillisecondsToDateTime(long timestamp)
    {
        var offset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        return offset.LocalDateTime;
    }

}
