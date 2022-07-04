using ASP.NET6WithRedisCache.Utilities;
using StackExchange.Redis;

namespace ASP.NET6WithRedisCache.Interface
{
    public interface IRepositoryHelper
    {
        IDatabase GetCacheDatabase(RedisDatabaseTarget target);
    }
}
