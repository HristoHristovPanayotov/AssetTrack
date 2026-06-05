using AssetTrack.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetTrack.Data.Configurations
{
    public class MaintenanceTicketConfiguration : IEntityTypeConfiguration<MaintenanceTicket>
    {
        public void Configure(EntityTypeBuilder<MaintenanceTicket> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Description).IsRequired().HasMaxLength(1000);
            builder.Property(t => t.RepairCost).HasColumnType("decimal(18,2)");

            builder.HasOne(t => t.Asset)
                .WithMany(a => a.MaintenanceTickets)
                .HasForeignKey(t => t.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
