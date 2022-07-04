using Newtonsoft.Json;
using StackExchange.Redis;

namespace ASP.NET6WithRedisCache.Utilities
{
    public class CacheMemberObject<T> : CacheMemberStringBase where T : class
    {
        private readonly Func<T, string[]> instanceInvalidationTags;

        private CacheMemberObject(string key, TimeSpan? defaultExpiration, Func<T, string[]> instanceInvalidationTags, string[] invalidationTags, RedisDatabaseTarget databaseTarget)
            : base(key, defaultExpiration, invalidationTags, databaseTarget)
        {
            this.instanceInvalidationTags = instanceInvalidationTags;
        }

        protected override bool UseSimpleInvalidationLogic => this.instanceInvalidationTags == null;

        public static CacheMemberObject<T> Persistent(string key, TimeSpan? defaultExpiration = null)
        {
            return new CacheMemberObject<T>(key, defaultExpiration, null, null, RedisDatabaseTarget.Persistent);
        }

        public static CacheMemberObject<T> Transient(string key, TimeSpan defaultExpiration, params string[] invalidationTags)
        {
            return new CacheMemberObject<T>(key, defaultExpiration, null, invalidationTags, RedisDatabaseTarget.Transient);
        }

        public static CacheMemberObject<T> Transient(string key, TimeSpan defaultExpiration, Func<T, string[]> instanceInvalidationTags, params string[] invalidationTags)
        {
            return new CacheMemberObject<T>(key, defaultExpiration, instanceInvalidationTags, invalidationTags, RedisDatabaseTarget.Transient);
        }

        public async Task<TOther> GetAsAsync<TOther>(IDatabase db) where TOther : class
        {
            RedisValue cached = await this.GetStringAsync(db);

            if (cached.IsNullOrEmpty)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<TOther>(cached);
        }

        public async Task<T> GetAsync(IDatabase db)
        {
            RedisValue cached = await this.GetStringAsync(db);

            if (cached.IsNullOrEmpty)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(cached);
        }

        public async Task SetAsync(IDatabase db, T value)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            await this.SetStringAsync(db, JsonConvert.SerializeObject(value, settings));

            if (this.instanceInvalidationTags != null)
            {
                await this.SetTagsAsync(db, instanceInvalidationTags(value));
            }
        }
    }
}
