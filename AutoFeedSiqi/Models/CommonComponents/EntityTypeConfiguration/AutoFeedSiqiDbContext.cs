using AutoFeedSiqi.Models.CommonComponents.Model;
using Microsoft.EntityFrameworkCore;

namespace AutoFeedSiqi.Models.CommonComponents.EntityTypeConfiguration
{
    public class AutoFeedSiqiDbContext : DbContext
    {
        public DbSet<Feed> Feeds { get; set; }
        public DbSet<Configuration> Configurations { get; set; }

        public AutoFeedSiqiDbContext(DbContextOptions<AutoFeedSiqiDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new FeedEntityTypeConfiguration().Configure(modelBuilder.Entity<Feed>());
            new ConfigurationEntityTypeConfiguration().Configure(modelBuilder.Entity<Configuration>());
        }
    }
}
