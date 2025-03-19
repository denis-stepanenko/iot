using IOTAPI.Models;
using NRedisTimeSeries;
using NRedisTimeSeries.Commands.Enums;
using NRedisTimeSeries.DataTypes;
using StackExchange.Redis;

namespace IOTAPI.Data.Repos;

public class ReadingRepo
{
    private readonly IDatabase _db;
    public ReadingRepo(RedisContext context)
    {
        _db = context.db;
    }

    public async Task CreateSeriesAsync(string name)
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

    public async Task<IEnumerable<Reading>> GetAllAsync(string topic, int count)
    {
        if (!await _db.KeyExistsAsync(topic))
            return new List<Reading>();

        IReadOnlyList<TimeSeriesTuple> result = await _db.TimeSeriesRevRangeAsync(topic, "-", "+", count: count);

        var items = result.Select(x => new Reading
        {
            Date = DateTimeHelper.UnixMillisecondsToDateTime(x.Time),
            Value = x.Val
        });

        return items.OrderBy(x => x.Date);
    }

    public async Task<Reading> AddAsync(string topic, double value)
    {
        var date = DateTime.Now;

        await _db.TimeSeriesAddAsync(topic, DateTimeHelper.DateTimeToUnixMilliseconds(date), value);

        return new Reading { Date = date, Value = value };
    }

    public async Task DeleteSeriesAsync(string name)
    {
        if (!await _db.KeyExistsAsync(name))
            return;

        await _db.KeyDeleteAsync(name);
    }
}