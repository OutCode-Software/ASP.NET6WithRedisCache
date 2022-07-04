using ASP.NET6WithRedisCache.Models;

namespace ASP.NET6WithRedisCache.Utilities
{
    public static class CacheKey
    {
        public static CacheMemberObject<List<Book>> Books() => CacheMemberObject<List<Book>>.Transient("book:all", TimeSpan.FromDays(1));
        public static CacheMemberObject<List<User>> Users() => CacheMemberObject<List<User>>.Transient("users:all", TimeSpan.FromMinutes(1));
    }
}
