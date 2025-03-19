using IOTAPI.Models;
using StackExchange.Redis;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using System.Text.Json;

namespace IOTAPI.Data.Repos;

public class ComponentRepo
{
    private readonly IDatabase _db;
    public ComponentRepo(RedisContext context)
    {
        _db = context.db;
    }

    public async Task<Component?> GetAsync(string id)
    {
        var result = await _db.JSON().GetAsync(key: "component:" + id);
        string json = result.ToString();

        if(string.IsNullOrEmpty(json))
            return null;

        var item = JsonSerializer.Deserialize<Component>(json);
        return item;
    }

    public async Task<List<Component>> GetAllAsync()
    {
        SearchResult result = await _db.FT().SearchAsync("idx:components", new Query("*"));

        List<string> jsonObjects = result.ToJson();

        var items = jsonObjects.Select(x => JsonSerializer.Deserialize<Component>(x) 
            ?? throw new ArgumentException("Невозможно десериализовать JSON"));

        // Use sorted set instead
        return items.OrderBy(x => x.CreatedAt).ToList();
    }

    public async Task CreateAsync(Component item)
    {
        await _db.JSON().SetAsync("component:" + item.Topic, "$", item);
    }

    public async Task DeleteAsync(string id)
    {
        await _db.JSON().DelAsync("component:" + id);
    }
}
