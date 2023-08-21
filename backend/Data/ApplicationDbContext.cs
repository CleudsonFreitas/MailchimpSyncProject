using Microsoft.EntityFrameworkCore;
using ContactsSync.Repository.Entities;

namespace ContactsSync.Repository
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}

        public DbSet<ContactEntity> Contacts => Set<ContactEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var folder = Environment.CurrentDirectory;
            options.UseSqlite($"Data Source={Path.Join(folder, "contacts.db")}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    } 
}
