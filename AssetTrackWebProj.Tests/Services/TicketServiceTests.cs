using AssetTrack.Data.Models;
using AssetTrack.Data.Models.Enums;
using AssetTrack.Services.Exceptions;
using AssetTrack.Services.Implementations;
using AssetTrack.Services.Models.Tickets;
using AssetTrack.Services.Repository;
using MockQueryable;

namespace AssetTrack.Tests.Services
{
    [TestFixture]
    public class TicketServiceTests
    {
        private Mock<IRepository<MaintenanceTicket>> _tickets = null!;
        private Mock<IRepository<Asset>> _assets = null!;
        private List<MaintenanceTicket> _ticketData = null!;
        private TicketService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _ticketData = new List<MaintenanceTicket>();

            _tickets = new Mock<IRepository<MaintenanceTicket>>();
            _tickets.Setup(r => r.AllAsNoTracking()).Returns(() => _ticketData.BuildMock());
            _tickets.Setup(r => r.All()).Returns(() => _ticketData.BuildMock());
            _tickets.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _tickets.Setup(r => r.AddAsync(It.IsAny<MaintenanceTicket>()))
                .Returns(Task.CompletedTask)
                .Callback<MaintenanceTicket>(t =>
                {
                    if (t.Id == 0) t.Id = _ticketData.Count + 1;
                    _ticketData.Add(t);
                });

            _assets = new Mock<IRepository<Asset>>();
            _assets.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _service = new TicketService(_tickets.Object, _assets.Object);
        }

        // ---------- REQUIRED TEST: filing a ticket flips asset to UnderMaintenance ----------
        [Test]
        public async Task CreateAsync_WhenTicketFiled_SetsAssetStatusToUnderMaintenance()
        {
            var asset = new Asset
            {
                Id = 1, Name = "Laptop", SerialNumber = "S1", Model = "M",
                Status = AssetStatus.Assigned
            };
            _assets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(asset);

            var model = new TicketInputModel
            {
                AssetId = 1,
                Description = "Screen flickers badly and shuts down."
            };

            await _service.CreateAsync(model);

            Assert.That(asset.Status, Is.EqualTo(AssetStatus.UnderMaintenance));
            _assets.Verify(r => r.Update(asset), Times.Once);
            _tickets.Verify(r => r.AddAsync(It.IsAny<MaintenanceTicket>()), Times.Once);
            _tickets.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void CreateAsync_WhenAssetMissing_ThrowsServiceValidationException()
        {
            _assets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync((Asset?)null);

            var model = new TicketInputModel { AssetId = 99, Description = "Broken keyboard." };

            Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.CreateAsync(model));
        }

        [Test]
        public async Task CreateAsync_WhenAssetRetired_DoesNotChangeStatus()
        {
            var asset = new Asset
            {
                Id = 2, Name = "Old Server", SerialNumber = "S2", Model = "M",
                Status = AssetStatus.Retired
            };
            _assets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(asset);

            var model = new TicketInputModel { AssetId = 2, Description = "Won't power on at all." };

            await _service.CreateAsync(model);

            Assert.That(asset.Status, Is.EqualTo(AssetStatus.Retired));
            _assets.Verify(r => r.Update(It.IsAny<Asset>()), Times.Never);
        }

        [Test]
        public void CreateAsync_WithEmptyDescription_Throws()
        {
            var asset = new Asset { Id = 3, Name = "PC", SerialNumber = "S3", Model = "M" };
            _assets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(asset);

            var model = new TicketInputModel { AssetId = 3, Description = "   " };

            Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.CreateAsync(model));
        }

        [Test]
        public void ResolveAsync_WithNegativeRepairCost_Throws()
        {
            Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.ResolveAsync(1, -10m));
        }

        [Test]
        public void ResolveAsync_WhenTicketNotFound_Throws()
        {
            _tickets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>()))
                .ReturnsAsync((MaintenanceTicket?)null);

            Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.ResolveAsync(5, 100m));
        }

        [Test]
        public async Task ResolveAsync_ClosesTicketAndReturnsUnassignedAssetToAvailable()
        {
            var ticket = new MaintenanceTicket
            {
                Id = 10, AssetId = 1, Description = "x",
                DateReported = DateTime.UtcNow, IsResolved = false
            };
            _ticketData.Add(ticket);
            _tickets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(ticket);

            var asset = new Asset
            {
                Id = 1, Name = "Laptop", SerialNumber = "S1", Model = "M",
                Status = AssetStatus.UnderMaintenance, AssignedUserId = null
            };
            _assets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(asset);

            await _service.ResolveAsync(10, 250m);

            Assert.That(ticket.IsResolved, Is.True);
            Assert.That(ticket.RepairCost, Is.EqualTo(250m));
            Assert.That(asset.Status, Is.EqualTo(AssetStatus.Available));
        }

        [Test]
        public async Task ResolveAsync_ReturnsAssignedAssetToAssignedStatus()
        {
            var ticket = new MaintenanceTicket
            {
                Id = 11, AssetId = 1, Description = "x",
                DateReported = DateTime.UtcNow, IsResolved = false
            };
            _ticketData.Add(ticket);
            _tickets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(ticket);

            var asset = new Asset
            {
                Id = 1, Name = "Laptop", SerialNumber = "S1", Model = "M",
                Status = AssetStatus.UnderMaintenance, AssignedUserId = "emp-1"
            };
            _assets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(asset);

            await _service.ResolveAsync(11, 0m);

            Assert.That(asset.Status, Is.EqualTo(AssetStatus.Assigned));
        }

        [Test]
        public async Task ResolveAsync_KeepsUnderMaintenanceWhenOtherOpenTicketsExist()
        {
            var ticket = new MaintenanceTicket
            {
                Id = 12, AssetId = 1, Description = "first", IsResolved = false,
                DateReported = DateTime.UtcNow
            };
            var otherOpen = new MaintenanceTicket
            {
                Id = 13, AssetId = 1, Description = "second", IsResolved = false,
                DateReported = DateTime.UtcNow
            };
            _ticketData.Add(ticket);
            _ticketData.Add(otherOpen);
            _tickets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(ticket);

            var asset = new Asset
            {
                Id = 1, Name = "Laptop", SerialNumber = "S1", Model = "M",
                Status = AssetStatus.UnderMaintenance
            };
            _assets.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(asset);

            await _service.ResolveAsync(12, 75m);

            Assert.That(asset.Status, Is.EqualTo(AssetStatus.UnderMaintenance));
        }

        [Test]
        public async Task GetOpenTicketCountAsync_CountsOnlyUnresolved()
        {
            _ticketData.Add(new MaintenanceTicket { Id = 1, AssetId = 1, Description = "a", IsResolved = false });
            _ticketData.Add(new MaintenanceTicket { Id = 2, AssetId = 1, Description = "b", IsResolved = true });
            _ticketData.Add(new MaintenanceTicket { Id = 3, AssetId = 2, Description = "c", IsResolved = false });

            var count = await _service.GetOpenTicketCountAsync();

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetTotalRepairExpenditureAsync_WhenEmpty_ReturnsZero()
        {
            var total = await _service.GetTotalRepairExpenditureAsync();
            Assert.That(total, Is.EqualTo(0m));
        }

        [Test]
        public async Task GetTotalRepairExpenditureAsync_SumsRepairCosts()
        {
            _ticketData.Add(new MaintenanceTicket { Id = 1, AssetId = 1, Description = "a", RepairCost = 100m });
            _ticketData.Add(new MaintenanceTicket { Id = 2, AssetId = 1, Description = "b", RepairCost = 50m });

            var total = await _service.GetTotalRepairExpenditureAsync();

            Assert.That(total, Is.EqualTo(150m));
        }

        [Test]
        public async Task GetTicketsForAssetAsync_ReturnsOnlyMatchingAsset()
        {
            _ticketData.Add(new MaintenanceTicket { Id = 1, AssetId = 1, Description = "a", DateReported = DateTime.UtcNow });
            _ticketData.Add(new MaintenanceTicket { Id = 2, AssetId = 2, Description = "b", DateReported = DateTime.UtcNow });

            var result = (await _service.GetTicketsForAssetAsync(1)).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].AssetId, Is.EqualTo(1));
        }

        [Test]
        public async Task GetOpenTicketsAsync_ReturnsOnlyUnresolved()
        {
            _ticketData.Add(new MaintenanceTicket { Id = 1, AssetId = 1, Description = "a", IsResolved = false, DateReported = DateTime.UtcNow });
            _ticketData.Add(new MaintenanceTicket { Id = 2, AssetId = 1, Description = "b", IsResolved = true, DateReported = DateTime.UtcNow });

            var result = (await _service.GetOpenTicketsAsync()).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].IsResolved, Is.False);
        }
    }
}
