# AssetTrack — IT Asset Management System

A multi-project **ASP.NET Core 8.0 MVC** enterprise application for documenting,
monitoring, and maintaining physical corporate IT hardware. Built for Visual
Studio 2022 with Entity Framework Core, ASP.NET Identity, a decoupled service
layer, a JSON Web API, and an NUnit/Moq test suite.

> **Note on the target framework.** The projects target **net8.0**, which is
> fully supported by Visual Studio 2022. If your machine has the .NET 10 SDK and
> you prefer it, you can change `<TargetFramework>net8.0</TargetFramework>` to
> `net10.0` in every `.csproj` and bump the EF Core / Identity package versions
> to the matching `10.0.*` line.

---

## Solution structure

```
AssetTrack.sln
├── AssetTrack.Data       (class library) entities, DbContext, fluent configs, seeding
├── AssetTrack.Services   (class library) interfaces, service logic, validation, view models, repository
├── AssetTrack.Web        (ASP.NET Core MVC) controllers, Razor views, Web API, DI wiring, static assets
└── AssetTrack.Tests      (NUnit) service-layer unit tests with Moq + MockQueryable
```

### The four domain entities
`ApplicationUser` (extends `IdentityUser`), `Asset`, `Category`, `MaintenanceTicket`.

### Architecture highlights
- **Thin controllers**: all business logic lives in `AssetService`, `CategoryService`,
  and `TicketService`, injected via native .NET DI.
- **Generic repository** (`IRepository<T>` / `EfRepository<T>`) so services are unit
  testable without a real database.
- **Custom Account controller** (Login / Register / Logout / AccessDenied) so
  registration captures `FirstName`, `LastName`, and `Department`.
- **Security**: `[Authorize(Roles = "Administrator")]` guards the entire
  `CategoriesController` and all write actions of `AssetsController`; global
  anti-forgery (CSRF) protection via `AutoValidateAntiforgeryTokenAttribute`;
  all data access uses parameterised LINQ (EF Core) — no raw SQL.

---

## Prerequisites
- **Visual Studio 2022** (17.8+) with the *ASP.NET and web development* workload, **or** the **.NET 8 SDK** + your editor of choice.
- **SQL Server LocalDB** (ships with Visual Studio). The default connection string in
  `AssetTrack.Web/appsettings.json` points at `(localdb)\MSSQLLocalDB`, database `AssetTrackDb`.
  Adjust it for a full SQL Server instance if you prefer.

---

## First-time setup & run

### 1. Restore packages
Open `AssetTrack.sln` in Visual Studio (NuGet restores automatically), or from the CLI:
```bash
dotnet restore
```

### 2. Create the initial EF Core migration  **(required)**
No migration files are shipped, so you must generate the first one. The migration
captures the schema **and** the seeded categories (Laptops, Monitors, Servers),
which are defined via `HasData` in `CategoryConfiguration`.

**Visual Studio — Package Manager Console** (set *Default project* to `AssetTrack.Data`):
```powershell
Add-Migration InitialCreate -StartupProject AssetTrack.Web
```

**…or the dotnet CLI** (install the tool once with `dotnet tool install --global dotnet-ef`):
```bash
dotnet ef migrations add InitialCreate --project AssetTrack.Data --startup-project AssetTrack.Web
```

### 3. Run the application
```bash
dotnet run --project AssetTrack.Web
```
(or press **F5** in Visual Studio.)

On startup the app automatically:
- applies any pending migrations (`Database.MigrateAsync()`), creating the database and seeding the categories, then
- seeds the `Administrator` and `Employee` roles and a default admin account (`IdentityDataSeeder`).

> If you prefer to apply the schema manually instead of on startup, run
> `Update-Database` (PMC) or `dotnet ef database update --project AssetTrack.Data --startup-project AssetTrack.Web` after step 2.

### 4. Sign in
| Role | Email | Password |
|------|-------|----------|
| Administrator | `admin@assettrack.local` | `Admin@123` |

New users who self-register through **Register** are placed in the **Employee** role.

> **Seeding design note.** Categories are seeded inside the EF migration (`HasData`).
> Roles and the admin **user** are seeded at runtime because Identity password hashing
> cannot be expressed reliably through `HasData`. This is intentional and documented in code.

---

## Running the tests
```bash
dotnet test
```
The `AssetTrack.Tests` project uses **NUnit 4 + Moq + MockQueryable** to fake the
repository layer entirely, so no database is required. Coverage targets the
business layer (`AssetService`, `TicketService`, `CategoryService`) and includes
the two mandated edge cases:

- creating an asset with a **negative value** fails validation, and
- filing a maintenance ticket flips the asset **Status → UnderMaintenance**.

To collect a coverage report:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## Views (15 Razor views)
| # | Controller | View | Purpose |
|---|------------|------|---------|
| 1 | Home | Index | Dashboard metric cards (total assets, open tickets, total value) |
| 2 | Home | Error | Custom styled 400 / 401 / 403 / 404 / 500 pages |
| 3 | Assets | Index | Paginated, sortable, live-search master grid |
| 4 | Assets | Details | **AJAX** `fetch()` to `/api/maintenance/{id}` for repair logs |
| 5 | Assets | Create | Admin-only; category + status + employee dropdowns |
| 6 | Assets | Edit | Admin-only; edit fields / reassign |
| 7 | Assets | Delete | Confirmation prompt |
| 8 | Categories | Index | Admin-only catalog list |
| 9 | Categories | Create | Admin-only |
| 10 | Categories | Edit | Admin-only |
| 11 | Tickets | Create | Employee "report an issue" |
| + | Assets | MyGear | Employee's assigned hardware |
| + | Tickets | Open | Admin queue with inline resolve |
| + | Account | Login / Register | Custom identity UI |

**Web API**: `GET /api/maintenance/{assetId}` (`MaintenanceApiController`) returns the
ticket history for an asset as JSON, consumed by the AJAX block on the Details page.

---

## Front-end libraries
Bootstrap 5, Bootstrap Icons, jQuery, and jQuery-validation are loaded from a CDN in
`_Layout.cshtml` / `_ValidationScriptsPartial.cshtml` so the app runs out of the box.
To vendor them locally instead (e.g. via LibMan), see `wwwroot/lib/README.txt`.
