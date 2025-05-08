using Microsoft.EntityFrameworkCore;
using ceng382_25_26_202211058.Models;

namespace ceng382_25_26_202211058.Data
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options)
            : base(options)
        {
        }

        public DbSet<Class> Classes { get; set; }
    }
} 