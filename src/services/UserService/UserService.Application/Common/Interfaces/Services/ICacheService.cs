namespace UserService.Application.Common.Interfaces.Services;

/// <summary>
/// Service interface for caching operations using Redis.
/// </summary>
public interface ICacheService
{
  /// <summary>
  /// Gets a cached value by key.
  /// </summary>
  /// <typeparam name="T">The value type</typeparam>
  /// <param name="key">The cache key</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The cached value if found, default otherwise</returns>
  Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

  /// <summary>
  /// Sets a value in cache with optional expiration.
  /// </summary>
  /// <typeparam name="T">The value type</typeparam>
  /// <param name="key">The cache key</param>
  /// <param name="value">The value to cache</param>
  /// <param name="expiration">Optional expiration time</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes a value from cache.
  /// </summary>
  /// <param name="key">The cache key</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task RemoveAsync(string key, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes multiple values from cache by pattern.
  /// </summary>
  /// <param name="pattern">The key pattern (supports wildcards)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if a key exists in cache.
  /// </summary>
  /// <param name="key">The cache key</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if exists, false otherwise</returns>
  Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
