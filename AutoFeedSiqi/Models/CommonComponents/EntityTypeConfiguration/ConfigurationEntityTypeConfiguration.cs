using AutoFeedSiqi.Models.CommonComponents.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFeedSiqi.Models.CommonComponents.EntityTypeConfiguration
{
    public class ConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<Configuration>
    {
        public void Configure(EntityTypeBuilder<Configuration> entity)
        {
            entity.HasKey(e => e.Id);
        }
    }
}
