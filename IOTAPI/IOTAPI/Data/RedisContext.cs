using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.Literals.Enums;
using StackExchange.Redis;

namespace IOTAPI.Data;

public class RedisContext
{
    public readonly IDatabase db;

    public RedisContext(string connectionString)
    {
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
        db = redis.GetDatabase();

        try
        {
            var schema = new Schema()
                .AddTextField(new FieldName("$.topic", "$.topic"));

            bool indexCreated = db.FT().Create(
                "idx:components",
                new FTCreateParams()
                    .On(IndexDataType.JSON)
                    .Prefix("component:"),
                    schema
            );
        }
        catch
        {
        }
    }
}
