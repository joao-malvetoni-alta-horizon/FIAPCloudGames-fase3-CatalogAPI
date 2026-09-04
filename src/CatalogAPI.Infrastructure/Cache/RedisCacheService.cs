using CatalogAPI.Application.Shared.Cache;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CatalogAPI.Infrastructure.Cache
{
    public class RedisCacheService(IConnectionMultiplexer connection) : ICacheService
    {
        private readonly IDatabase _cache = connection.GetDatabase();

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> buscarDados, TimeSpan expiracao, CancellationToken cancellationToken = default)
        {
            var cached = await _cache.StringGetAsync(key);
            if (cached.HasValue)
                return JsonSerializer.Deserialize<T>((string)cached!);

            var dados = await buscarDados();
            if (dados is not null)
                await _cache.StringSetAsync(key, JsonSerializer.Serialize(dados), expiracao);

            return dados;
        }

        public async Task SetAsync<T>(string key, T valor, TimeSpan expiracao, CancellationToken cancellationToken = default)
            => await _cache.StringSetAsync(key, JsonSerializer.Serialize(valor), expiracao);

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
            => await _cache.KeyDeleteAsync(key);
    }
}
