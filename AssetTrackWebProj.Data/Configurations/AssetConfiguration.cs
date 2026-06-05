using AssetTrack.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetTrack.Data.Configurations
{
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
            builder.Property(a => a.SerialNumber).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Model).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Value).HasColumnType("decimal(18,2)");

            // SerialNumber must be unique across the inventory.
            builder.HasIndex(a => a.SerialNumber).IsUnique();

            // Category (one-to-many). Restrict delete so categories with assets cannot be removed.
            builder.HasOne(a => a.Category)
                .WithMany(c => c.Assets)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // AssignedUser (one-to-many, optional). Setting null on user delete keeps the asset.
            builder.HasOne(a => a.AssignedUser)
                .WithMany(u => u.AssignedAssets)
                .HasForeignKey(a => a.AssignedUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
