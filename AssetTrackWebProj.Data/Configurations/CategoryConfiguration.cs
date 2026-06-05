using AssetTrack.Data.Models;
using AssetTrack.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetTrack.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Description).HasMaxLength(500);

            builder.HasIndex(c => c.Name).IsUnique();

            // Seed default categories straight into the migration (satisfies the seeding requirement).
            builder.HasData(
                new Category { Id = SeedConstants.LaptopsCategoryId, Name = "Laptops", Description = "Portable corporate computers." },
                new Category { Id = SeedConstants.MonitorsCategoryId, Name = "Monitors", Description = "External displays and screens." },
                new Category { Id = SeedConstants.ServersCategoryId, Name = "Servers", Description = "Rack and tower server hardware." }
            );
        }
    }
}
