using Microsoft.EntityFrameworkCore;
using Models;
using DotNetEnv;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var envPath = Path.Combine(AppContext.BaseDirectory, "BotTG", ".env");
            Env.Load(envPath);
            string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            optionsBuilder.UseSqlServer(connectionString);

        } // "Data Source=WIN-57GSVVQFVLA;Initial Catalog=MyBotDB;Integrated Security=True;Pooling=False;Encrypt=False;Trust Server Certificate=True"
    }
}
