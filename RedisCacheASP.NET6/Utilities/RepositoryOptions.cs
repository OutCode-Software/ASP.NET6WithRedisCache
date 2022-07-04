namespace ASP.NET6WithRedisCache.Utilities
{
    public sealed class RepositoryOptions
    {
        public byte[] AesKey { get; set; }

        public string ConnectionString { get; set; }

        public string RedisConfiguration { get; set; }

        public int RedisPersistentDatabaseId { get; set; } = 1;

        public int RedisTransientDatabaseId { get; set; } = 0;      

        public bool UseSqlServiceToken { get; set; } = true;
    }
}
