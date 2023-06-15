using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class SearchEngineDbContext : DbContext
    {
        public SearchEngineDbContext(DbContextOptions<SearchEngineDbContext> options)
            : base(options)
        {
        }
        public DbSet<Links> links { get; set; }
        public DbSet<Indexes> indexes { get; set; }
        public DbSet<PostingList> postingList { get; set; }
    }
}