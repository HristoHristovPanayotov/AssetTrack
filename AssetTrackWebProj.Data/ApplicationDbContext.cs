using System.Reflection;
using AssetTrack.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AssetTrack.Data
{
    /// <summary>
    /// Application database context. Inherits from IdentityDbContext so the custom
    /// ApplicationUser and the ASP.NET Identity schema live in the same database.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<MaintenanceTicket> MaintenanceTickets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity tables must be configured first.
            base.OnModelCreating(builder);

            // Apply every IEntityTypeConfiguration in this assembly (fluent API mapping).
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
