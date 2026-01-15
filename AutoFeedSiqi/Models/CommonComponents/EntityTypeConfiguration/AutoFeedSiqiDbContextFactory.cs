using Microsoft.EntityFrameworkCore.Design;

namespace AutoFeedSiqi.Models.CommonComponents.EntityTypeConfiguration
{
    /// <summary>
    /// 用于EF Core Tools识别
    /// </summary>
    public class AutoFeedSiqiDbContextFactory : IDesignTimeDbContextFactory<AutoFeedSiqiDbContext>
    {
        public AutoFeedSiqiDbContext CreateDbContext(string[] args) => new(DatabaseUtil.DbContextOptions);
    }
}
