using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogAPI.Application.Shared.Cache
{
    public interface ICacheService
    {
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> buscarDados, TimeSpan expiracao, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T valor, TimeSpan expiracao, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}
