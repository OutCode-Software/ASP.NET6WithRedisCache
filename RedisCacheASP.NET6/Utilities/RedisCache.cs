using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ASP.NET6WithRedisCache.Utilities
{
    public sealed class RedisCache
    {
        private readonly Lazy<ConnectionMultiplexer> connectionMultiplexer;

        public RedisCache(IOptions<RepositoryOptions> options)
        {
            this.connectionMultiplexer = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options.Value.RedisConfiguration));
        }
        internal ConnectionMultiplexer Connection => connectionMultiplexer.Value;
    }
}
