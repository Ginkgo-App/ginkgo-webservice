using APICore;
using APICore.Entities;
using Microsoft.EntityFrameworkCore;
using WebMvcPluginUser.Entities;

namespace WebMvcPluginUser.DBContext
{
    public class PostgreSQLContext : DbContext
    {
        public PostgreSQLContext(DbContextOptions<PostgreSQLContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseNpgsql(Vars.CONNECTION_STRING);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AuthProvider> AuthProviders { get; set; }
    }
}
