namespace AssetTrack.Data.Seeding
{
    /// <summary>
    /// Stable, well-known identifiers used both for migration seeding (HasData)
    /// and for the runtime Identity seeder.
    /// </summary>
    public static class SeedConstants
    {
        public const string AdministratorRoleName = "Administrator";
        public const string EmployeeRoleName = "Employee";

        // Fixed GUIDs so HasData produces deterministic, idempotent migrations.
        public const string AdministratorRoleId = "f1b8d2c0-1111-4a11-9c11-aaaaaaaaaaaa";
        public const string EmployeeRoleId = "f1b8d2c0-2222-4a22-9c22-bbbbbbbbbbbb";

        public const string AdministratorUserId = "f1b8d2c0-3333-4a33-9c33-cccccccccccc";
        public const string AdministratorEmail = "admin@assettrack.local";
        public const string AdministratorDefaultPassword = "Admin@123";

        // Category seed ids
        public const int LaptopsCategoryId = 1;
        public const int MonitorsCategoryId = 2;
        public const int ServersCategoryId = 3;
    }
}
