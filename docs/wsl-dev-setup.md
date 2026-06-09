# Running AssetTrack under WSL2 (Smart App Control workaround)

Smart App Control (SAC) is **Enforced** on this Windows machine. It blocks every
locally-built unsigned binary (`.exe` **and** `.dll`), so the app cannot be run or
debugged natively on Windows without turning SAC off (which is irreversible).

SAC only governs **Windows** PE files. Running the app inside **WSL2** sidesteps it
entirely — the Linux `dotnet` host and your compiled assemblies are never evaluated
by SAC. This guide sets that up.

The database also has to move: `(localdb)\MSSQLLocalDB` is Windows-only and uses
named pipes, which WSL cannot reach. We run SQL Server in a container inside WSL
instead (see [`docker-compose.yml`](../docker-compose.yml)).

---

## Step 1 — Install WSL2 + Ubuntu  (Windows, **elevated**, requires reboot)

Open **PowerShell as Administrator** and run:

```powershell
wsl --install -d Ubuntu
```

This enables the required Windows features, installs the Ubuntu distro, and then
**requires a reboot**. After rebooting, an Ubuntu window opens and prompts you to
create a **UNIX username and password** — complete that. Verify with:

```powershell
wsl --list --verbose      # Ubuntu should show VERSION 2
wsl --status              # Default Version: 2
```

Everything from here runs **inside the Ubuntu (WSL) shell** unless noted.

---

## Step 2 — Install the .NET 10 SDK in Ubuntu

```bash
# Microsoft package feed
sudo apt-get update && sudo apt-get install -y wget
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O /tmp/pmc.deb
sudo dpkg -i /tmp/pmc.deb
sudo apt-get update

# .NET 10 SDK
sudo apt-get install -y dotnet-sdk-10.0
dotnet --info     # confirm SDK 10.0.x

# EF Core CLI (for migrations) + make it visible on PATH
dotnet tool install --global dotnet-ef
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.bashrc
source ~/.bashrc
```

> If `dotnet-sdk-10.0` isn't yet in the apt feed for your Ubuntu release, use the
> official install script instead:
> `wget https://dot.net/v1/dotnet-install.sh -O /tmp/d.sh && bash /tmp/d.sh --channel 10.0`
> then add `~/.dotnet` to PATH.

---

## Step 3 — Install Docker engine in Ubuntu and start SQL Server

```bash
# Docker engine (not Docker Desktop)
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER
# log out/in of the WSL shell (or run: newgrp docker) so the group takes effect
sudo service docker start
```

Edit the SA password in `docker-compose.yml` if you like, then from the repo root
**inside WSL** (see Step 4 for the path), start the database:

```bash
docker compose up -d
docker ps            # assettrack-sql should be running
```

---

## Step 4 — Get the repo and point the app at the container DB

Your code currently lives on the Windows drive, reachable from WSL at
`/mnt/c/Users/Hristo/source/repos/AssetTrackWebProj`. You can build there, but for
much faster file I/O, cloning into the Linux filesystem is recommended:

```bash
cd ~
git clone https://github.com/HristoHristovPanayotov/AssetTrack.git
cd AssetTrack
```

Override the connection string **via environment variable** (keeps the committed
Windows `appsettings.json` untouched). The `__` maps to the
`ConnectionStrings:DefaultConnection` config key:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=AssetTrackDb;User Id=sa;Password=Your_strong!Passw0rd;TrustServerCertificate=True"
```

(Use the same password you set in `docker-compose.yml`.)

---

## Step 5 — Create the schema and run

```bash
# Apply the existing InitialCreate migration to the container DB
dotnet ef database update \
  --project AssetTrackWebProj.Data \
  --startup-project AssetTrackWebProj

# Run the app
dotnet run --project AssetTrackWebProj
```

`Program.cs` also calls `MigrateAsync()` on startup, so `dotnet run` alone will
create/seed the DB too — the explicit `database update` is just a clean checkpoint.

Browse to the URL it prints (e.g. `http://localhost:5xxx`). Because WSL2 forwards
localhost to Windows, you can open it in your Windows browser.

---

## Debugging

- **VS Code** (recommended): install the **WSL** and **C# Dev Kit** extensions,
  then `code .` from the WSL shell. Press F5 — debugging runs Linux-side, SAC-free.
- **Visual Studio**: full VS debugging targets Windows binaries, which SAC blocks.
  Use VS Code + WSL for the SAC-free debug loop.

---

## Notes

- Keep SAC **Enforced** — nothing here disables it.
- Stop the DB when done: `docker compose down` (data persists in the named volume).
- The Windows-side `appsettings.json` still points at LocalDB; only the WSL
  environment variable redirects to the container. No committed file conflicts.
