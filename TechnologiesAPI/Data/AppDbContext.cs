using Microsoft.EntityFrameworkCore;
using Models;
using DotNetEnv;
using System.ComponentModel.Design;

namespace Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Technology> Technologies { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<UsersTechnologies> UsersTechnologies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Technology>()
                .HasMany(t => t.Questions)
                .WithOne(q => q.Technology)
                .HasForeignKey(q => q.TechnologyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Technology>()
                .HasIndex(t => t.Title)
                .IsUnique();

            modelBuilder.Entity<Technology>()
                .HasMany(t => t.ChildTechnologies)
                .WithOne(t => t.ParentTechnology)
                .HasForeignKey(t => t.ParentTechnologyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Question>()
                .HasMany(t => t.AnswerOption)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasIndex(q => q.ShortName)
                .IsUnique();

            modelBuilder.Entity<Question>()
                .HasIndex(q => q.Text)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasKey(u => u.ChatId);

            modelBuilder.Entity<User>()
                .Property(u => u.ChatId)
                .ValueGeneratedNever();

            modelBuilder.Entity<UsersTechnologies>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTechnologies)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UsersTechnologies>()
                .HasOne(ut => ut.Technology)
                .WithMany(t => t.UsersTechnologies)
                .HasForeignKey(ut => ut.TechnologyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UsersTechnologies>()
                .Property(ut => ut.IsCompleted)
                .HasDefaultValue(false);


            base.OnModelCreating(modelBuilder);
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var envPath = Path.Combine(AppContext.BaseDirectory, "BotTG", ".env");
        //    Env.Load(envPath);
        //    string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        //    optionsBuilder.UseSqlServer(connectionString);

        //} // "Data Source=WIN-57GSVVQFVLA;Initial Catalog=MyBotDB;Integrated Security=True;Pooling=False;Encrypt=False;Trust Server Certificate=True"
    }
}
