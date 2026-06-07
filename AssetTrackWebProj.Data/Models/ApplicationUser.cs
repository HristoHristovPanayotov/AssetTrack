using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AssetTrack.Data.Models
{
    /// <summary>
    /// Application user that extends the framework IdentityUser with corporate domain data.
    /// A user can hold many assets simultaneously (one-to-many).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = null!;

        public ICollection<Asset> AssignedAssets { get; set; } = new HashSet<Asset>();

        public string FullName => $"{FirstName} {LastName}";
    }
}
