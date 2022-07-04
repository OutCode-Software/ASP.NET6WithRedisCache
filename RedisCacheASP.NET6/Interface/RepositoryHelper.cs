using Microsoft.Extensions.Options;
using ASP.NET6WithRedisCache.Utilities;
using StackExchange.Redis;

namespace ASP.NET6WithRedisCache.Interface
{
    public sealed class RepositoryHelper: IRepositoryHelper
    {
        private readonly RedisCache _cache;
        private readonly RepositoryOptions _options;
        public RepositoryHelper(IOptions<RepositoryOptions> options, RedisCache cache)
        {          
            _cache = cache;
            _options = options.Value;
        }
        public IDatabase GetCacheDatabase(RedisDatabaseTarget target) => _cache.Connection.GetDatabase(this.GetDatabaseId(target));
        private int GetDatabaseId(RedisDatabaseTarget target)
        {
            return target switch
            {
                RedisDatabaseTarget.Transient => _options.RedisTransientDatabaseId,
                RedisDatabaseTarget.Persistent => _options.RedisPersistentDatabaseId,
                _ => throw new NotSupportedException(),
            };
        }
    }
}
