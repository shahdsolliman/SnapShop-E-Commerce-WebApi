using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Services.Contract
{
    public interface IResponseCacheService
    {
        Task CacheResponseAsync(string chacheKey, object response, TimeSpan timeToLive);
        Task<string?> GetChachedResponseAsync(string cacheKey);
    }
}
