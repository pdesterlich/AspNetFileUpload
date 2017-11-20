using Microsoft.EntityFrameworkCore;

namespace AspNetFileUpload.Models
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options): base(options)
        {
            
        }
        
        public DbSet<Fotografia> Fotografie { get; set; }
    }
}