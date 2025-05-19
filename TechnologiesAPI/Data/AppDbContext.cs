using Microsoft.EntityFrameworkCore;
using Models;
using DotNetEnv;
using System.ComponentModel.Design;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Technology> Technologies { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Technology>()
                .HasMany(t => t.Questions)
                .WithOne(q => q.Technology)
                .HasForeignKey(q => q.TechnologyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(t => t.AnswerOption)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var envPath = Path.Combine(AppContext.BaseDirectory, "BotTG", ".env");
            Env.Load(envPath);
            string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            optionsBuilder.UseSqlServer(connectionString);

        } 
    }
}
