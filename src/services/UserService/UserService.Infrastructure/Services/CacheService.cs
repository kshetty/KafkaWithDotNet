using System.Text.Json;
using StackExchange.Redis;
using UserService.Application.Common.Interfaces.Services;

namespace UserService.Infrastructure.Services;

/// <summary>
/// Redis-based cache service implementation.
/// </summary>
public sealed class CacheService : ICacheService
{
  private readonly IConnectionMultiplexer _redis;
  private readonly IDatabase _database;

  public CacheService(IConnectionMultiplexer redis)
  {
    _redis = redis;
    _database = redis.GetDatabase();
  }

  public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
  {
    var value = await _database.StringGetAsync(key);

    if (value.IsNullOrEmpty)
    {
      return default;
    }

    return JsonSerializer.Deserialize<T>(value.ToString());
  }

  public async Task SetAsync<T>(
      string key,
      T value,
      TimeSpan? expiry = null,
      CancellationToken cancellationToken = default)
  {
    var serialized = JsonSerializer.Serialize(value);
    await _database.StringSetAsync(key, serialized, expiry);
  }

  public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
  {
    await _database.KeyDeleteAsync(key);
  }

  public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
  {
    var endpoints = _redis.GetEndPoints();
    var server = _redis.GetServer(endpoints.First());

    var keys = server.Keys(pattern: pattern).ToArray();

    if (keys.Length > 0)
    {
      await _database.KeyDeleteAsync(keys);
    }
  }

  public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
  {
    return await _database.KeyExistsAsync(key);
  }
}
