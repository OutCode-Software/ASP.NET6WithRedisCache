using System;
using System.Collections.Generic;

namespace ASP.NET6WithRedisCache.Models
{
    public partial class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public DateTimeOffset PublishDate { get; set; }
    }
}
