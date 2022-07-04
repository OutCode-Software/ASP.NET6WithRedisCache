using StackExchange.Redis;

namespace ASP.NET6WithRedisCache.Utilities
{
    public abstract class CacheMemberBase
    {
        private readonly RedisDatabaseTarget databaseTarget;
        private readonly TimeSpan? defaultExpiration;
        private readonly string[] invalidationTags;
        private readonly string key;

        protected CacheMemberBase(string key, TimeSpan? defaultExpiration, string[] invalidationTags, RedisDatabaseTarget databaseTarget)
        {
            this.defaultExpiration = defaultExpiration;
            this.invalidationTags = invalidationTags;
            this.key = key;
            this.databaseTarget = databaseTarget;
        }

        public RedisDatabaseTarget DatabaseTarget => this.databaseTarget;

        public TimeSpan? DefaultExpiration => this.defaultExpiration;

        public bool HasTags => this.invalidationTags != null && this.invalidationTags.Length > 0;

        public string Key => this.key;

        protected virtual bool UseSimpleInvalidationLogic => true;

        protected async Task<bool> IsValidAsync(IDatabase db)
        {
            if (this.UseSimpleInvalidationLogic)
            {
                if (this.invalidationTags == null || this.invalidationTags.Length == 0)
                {
                    return true;
                }
            }

            HashEntry[] entries = await db.HashGetAllAsync($"tags:{this.Key}");
            var tags = new List<string>(this.UseSimpleInvalidationLogic ? this.invalidationTags : entries.Select(e => (string)e.Name));

            List<RedisKey> keys = new List<RedisKey>(tags.Count);
            foreach (string tag in tags)
            {
                keys.Add($"tag:{tag}");
            }

            RedisValue[] values = await db.StringGetAsync(keys.ToArray());

            for (int i = 0; i < tags.Count; i++)
            {
                RedisValue expected = entries.FirstOrDefault(e => e.Name == tags[i]).Value;
                RedisValue actual = values[i];
                if (actual.IsNull || expected != actual)
                {
                    return false;
                }
            }

            return true;
        }

        protected async Task SetTagsAsync(IDatabase db)
        {
            if (this.invalidationTags == null || this.invalidationTags.Length == 0)
            {
                return;
            }

            await this.SetTagsAsync(db, this.invalidationTags);
        }

        protected async Task SetTagsAsync(IDatabase db, string[] tags)
        {
            List<HashEntry> entries = new List<HashEntry>();

            ITransaction tx;

            foreach (string tag in tags)
            {
                string tagKey = $"tag:{tag}";
                tx = db.CreateTransaction();
                Task setTask = tx.StringSetAsync(tagKey, Guid.NewGuid().ToString(), when: When.NotExists, expiry: TimeSpan.FromDays(14));
                Task<RedisValue> getTask = tx.StringGetAsync(tagKey);
                await tx.ExecuteAsync();

                entries.Add(new HashEntry(tag, getTask.Result));
            }

            string tagsKey = $"tags:{this.Key}";
            tx = db.CreateTransaction();
            _ = tx.HashSetAsync(tagsKey, entries.ToArray());
            _ = tx.KeyExpireAsync(tagsKey, TimeSpan.FromDays(14));
            await tx.ExecuteAsync();
        }
    }
}
