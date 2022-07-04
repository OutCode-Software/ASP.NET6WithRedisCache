using StackExchange.Redis;

namespace ASP.NET6WithRedisCache.Utilities
{
    public abstract class CacheMemberStringBase : CacheMemberBase
    {
        protected CacheMemberStringBase(string key, TimeSpan? defaultExpiration, string[] invalidationTags, RedisDatabaseTarget databaseTarget)
            : base(key, defaultExpiration, invalidationTags, databaseTarget)
        {
        }
        protected async Task<RedisValue> GetStringAsync(IDatabase db)
        {
            if (!(await this.IsValidAsync(db)))
            {
                return RedisValue.Null;
            }

            return await db.StringGetAsync(this.Key);
        }

        protected async Task<RedisValue> GetStringAsync(IDatabase db, TimeSpan updateExpiry)
        {
            if (!(await this.IsValidAsync(db)))
            {
                return RedisValue.Null;
            }

            ITransaction tx = db.CreateTransaction();
            _ = tx.KeyExpireAsync(this.Key, updateExpiry);
            Task<RedisValue> getTask = tx.StringGetAsync(this.Key);
            await tx.ExecuteAsync();
            return getTask.Result;
        }

        protected async Task SetStringAsync(IDatabase db, RedisValue value, TimeSpan? expiry = null)
        {
            expiry = expiry ?? this.DefaultExpiration;
            await this.SetTagsAsync(db);
            await db.StringSetAsync(this.Key, value, expiry);
        }
    }
}
