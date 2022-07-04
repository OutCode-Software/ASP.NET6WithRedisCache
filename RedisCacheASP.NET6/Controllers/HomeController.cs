using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using ASP.NET6WithRedisCache.Interface;
using ASP.NET6WithRedisCache.Models;
using ASP.NET6WithRedisCache.Utilities;
using StackExchange.Redis;
using BenchmarkDotNet.Attributes;

namespace ASP.NET6WithRedisCache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly CoreDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private readonly IRepositoryHelper _repositoryHelper;

        public HomeController(CoreDbContext dbContext, IDistributedCache cache, IRepositoryHelper repositoryHelper)
        {
            _dbContext = dbContext;
            _cache = cache;
            _repositoryHelper = repositoryHelper;
        }

        [HttpGet]
        [Benchmark]
        [Route("GetAllbooks/{enableCache}")]
        public async Task<List<Book>> GetAllbooks(bool enableCache)
        {
            if (!enableCache)
            {
                return _dbContext.Books.ToList();
            }
            var cacheKey = CacheKey.Books();
            IDatabase cacheDb = _repositoryHelper.GetCacheDatabase(cacheKey.DatabaseTarget);

            var model = await cacheKey.GetAsAsync<List<Book>>(cacheDb);
            if (model == null)
            {
                var books = _dbContext.Books.ToList();
                await cacheKey.SetAsync(cacheDb, books);
                return books;
            }
            else
            {
                return model;
            }
        }

        [HttpGet]
        [Route("GetAllUser/{enableCache}")]
        public async Task<List<User>> GetAllUser(bool enableCache)
        {
            if (!enableCache)
            {
                return _dbContext.Users.ToList();
            }
            var cacheKey = CacheKey.Users();
            IDatabase cacheDb = _repositoryHelper.GetCacheDatabase(cacheKey.DatabaseTarget);

            var model = await cacheKey.GetAsAsync<List<User>>(cacheDb);
            if (model == null)
            {
                var users = _dbContext.Users.ToList();
                await cacheKey.SetAsync(cacheDb, users);
                return users;
            }
            else
            {
                return model;
            }
        }
    }
}
