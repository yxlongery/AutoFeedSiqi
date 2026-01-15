using AutoFeedSiqi.Models.CommonComponents.EntityTypeConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AutoFeedSiqi.Models.CommonComponents
{
    public static class DatabaseUtil
    {
        public static DbContextOptions<AutoFeedSiqiDbContext> DbContextOptions { get; } = new DbContextOptionsBuilder<AutoFeedSiqiDbContext>()
            .UseSqlite($@"Data Source={Path.Combine("data", "feedsiqi.db")}")
            .UseSnakeCaseNamingConvention()
            .Options;
        public static PooledDbContextFactory<AutoFeedSiqiDbContext> PooledDbContextFactory { get; } = new(DbContextOptions);
    }
}
