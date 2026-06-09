using AssetTrack.Data.Models;
using AssetTrack.Data.Models.Enums;
using AssetTrack.Services.Exceptions;
using AssetTrack.Services.Implementations;
using AssetTrack.Services.Models.Assets;
using AssetTrack.Services.Repository;
using MockQueryable;
using Moq;
using NUnit.Framework;

namespace AssetTrack.Tests.Services
{
    [TestFixture]
    public class AssetServiceTests
    {
        private Mock<IRepository<Asset>> _repo = null!;
        private List<Asset> _data = null!;
        private AssetService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _data = new List<Asset>();
            _repo = new Mock<IRepository<Asset>>();

            // AllAsNoTracking()/All() return an async-capable IQueryable built from the
            // in-memory list, so EF async operators (AnyAsync/CountAsync/...) work.
            _repo.Setup(r => r.AllAsNoTracking()).Returns(() => _data.BuildMock());
            _repo.Setup(r => r.All()).Returns(() => _data.BuildMock());
            _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _repo.Setup(r => r.AddAsync(It.IsAny<Asset>()))
                .Returns(Task.CompletedTask)
                .Callback<Asset>(a =>
                {
                    if (a.Id == 0) a.Id = _data.Count + 1;
                    _data.Add(a);
                });

            _service = new AssetService(_repo.Object);
        }

        private static AssetFormModel ValidForm() => new()
        {
            Name = "Dell Latitude",
            SerialNumber = "SN-1001",
            Model = "Latitude 5540",
            PurchaseDate = new DateTime(2024, 1, 1),
            Value = 1500m,
            Status = AssetStatus.Available,
            CategoryId = 1
        };

        // ---------- REQUIRED TEST: negative value must be rejected ----------
        [Test]
        public void CreateAsync_WithNegativeValue_ThrowsServiceValidationException()
        {
            var model = ValidForm();
            model.Value = -50m;

            var ex = Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.CreateAsync(model));

            Assert.That(ex!.Message, Does.Contain("negative"));
            _repo.Verify(r => r.AddAsync(It.IsAny<Asset>()), Times.Never);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public void CreateAsync_WithValueAboveMaximum_ThrowsServiceValidationException()
        {
            var model = ValidForm();
            model.Value = 5_000_000m;

            var ex = Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.CreateAsync(model));

            Assert.That(ex!.Message, Does.Contain("exceed"));
            _repo.Verify(r => r.AddAsync(It.IsAny<Asset>()), Times.Never);
        }

        [Test]
        public void CreateAsync_WithDuplicateSerialNumber_ThrowsServiceValidationException()
        {
            _data.Add(new Asset
            {
                Id = 1,
                Name = "Existing",
                SerialNumber = "SN-1001",
                Model = "X",
                CategoryId = 1,
                Value = 100m
            });

            var model = ValidForm(); // same serial SN-1001

            var ex = Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.CreateAsync(model));

            Assert.That(ex!.Message, Does.Contain("already exists"));
        }

        [Test]
        public async Task CreateAsync_WithValidData_AddsAssetAndSaves()
        {
            var model = ValidForm();

            var newId = await _service.CreateAsync(model);

            Assert.That(newId, Is.GreaterThan(0));
            Assert.That(_data, Has.Count.EqualTo(1));
            Assert.That(_data[0].SerialNumber, Is.EqualTo("SN-1001"));
            _repo.Verify(r => r.AddAsync(It.IsAny<Asset>()), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CreateAsync_WithAssignedUserAndAvailableStatus_PromotesToAssigned()
        {
            var model = ValidForm();
            model.Status = AssetStatus.Available;
            model.AssignedUserId = "user-1";

            await _service.CreateAsync(model);

            Assert.That(_data[0].Status, Is.EqualTo(AssetStatus.Assigned));
            Assert.That(_data[0].AssignedUserId, Is.EqualTo("user-1"));
        }

        [Test]
        public void UpdateAsync_WhenAssetNotFound_ThrowsServiceValidationException()
        {
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<object[]>()))
                .ReturnsAsync((Asset?)null);

            var model = ValidForm();
            model.Id = 99;

            Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.UpdateAsync(model));
        }

        [Test]
        public async Task UpdateAsync_WithValidData_UpdatesAndSaves()
        {
            var existing = new Asset
            {
                Id = 5,
                Name = "Old",
                SerialNumber = "SN-OLD",
                Model = "M",
                CategoryId = 1,
                Value = 10m,
                Status = AssetStatus.Available
            };
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(existing);

            var model = ValidForm();
            model.Id = 5;
            model.Name = "Updated Name";
            model.SerialNumber = "SN-NEW";

            await _service.UpdateAsync(model);

            Assert.That(existing.Name, Is.EqualTo("Updated Name"));
            Assert.That(existing.SerialNumber, Is.EqualTo("SN-NEW"));
            _repo.Verify(r => r.Update(existing), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void DeleteAsync_WhenAssetNotFound_ThrowsServiceValidationException()
        {
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<object[]>()))
                .ReturnsAsync((Asset?)null);

            Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.DeleteAsync(123));
        }

        [Test]
        public async Task DeleteAsync_WhenFound_DeletesAndSaves()
        {
            var existing = new Asset { Id = 7, Name = "Gone", SerialNumber = "S", Model = "M" };
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(existing);

            await _service.DeleteAsync(7);

            _repo.Verify(r => r.Delete(existing), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void AssignToUserAsync_WithEmptyUserId_Throws()
        {
            Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.AssignToUserAsync(1, ""));
        }

        [Test]
        public async Task AssignToUserAsync_WithValidData_SetsAssignmentAndStatus()
        {
            var existing = new Asset
            {
                Id = 3, Name = "Laptop", SerialNumber = "S3", Model = "M",
                Status = AssetStatus.Available
            };
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(existing);

            await _service.AssignToUserAsync(3, "emp-9");

            Assert.That(existing.AssignedUserId, Is.EqualTo("emp-9"));
            Assert.That(existing.Status, Is.EqualTo(AssetStatus.Assigned));
        }

        [Test]
        public async Task UnassignAsync_ClearsAssignmentAndReturnsToAvailable()
        {
            var existing = new Asset
            {
                Id = 4, Name = "Monitor", SerialNumber = "S4", Model = "M",
                AssignedUserId = "emp-1", Status = AssetStatus.Assigned
            };
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(existing);

            await _service.UnassignAsync(4);

            Assert.That(existing.AssignedUserId, Is.Null);
            Assert.That(existing.Status, Is.EqualTo(AssetStatus.Available));
        }

        [Test]
        public async Task GetTotalInventoryValueAsync_WhenEmpty_ReturnsZero()
        {
            var total = await _service.GetTotalInventoryValueAsync();
            Assert.That(total, Is.EqualTo(0m));
        }

        [Test]
        public async Task GetTotalInventoryValueAsync_WithAssets_ReturnsSum()
        {
            _data.Add(new Asset { Id = 1, Name = "A", SerialNumber = "1", Model = "M", Value = 100m });
            _data.Add(new Asset { Id = 2, Name = "B", SerialNumber = "2", Model = "M", Value = 250m });

            var total = await _service.GetTotalInventoryValueAsync();

            Assert.That(total, Is.EqualTo(350m));
        }

        [Test]
        public async Task GetTotalAssetCountAsync_ReturnsCount()
        {
            _data.Add(new Asset { Id = 1, Name = "A", SerialNumber = "1", Model = "M" });
            _data.Add(new Asset { Id = 2, Name = "B", SerialNumber = "2", Model = "M" });

            var count = await _service.GetTotalAssetCountAsync();

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public async Task ExistsAsync_ReturnsTrueWhenPresent()
        {
            _data.Add(new Asset { Id = 42, Name = "A", SerialNumber = "1", Model = "M" });

            Assert.That(await _service.ExistsAsync(42), Is.True);
            Assert.That(await _service.ExistsAsync(99), Is.False);
        }

        [Test]
        public async Task GetAssetsForUserAsync_ReturnsOnlyAssetsForThatUser()
        {
            var cat = new Category { Id = 1, Name = "Laptops" };
            _data.Add(new Asset { Id = 1, Name = "Mine", SerialNumber = "1", Model = "M", AssignedUserId = "u1", Category = cat, CategoryId = 1 });
            _data.Add(new Asset { Id = 2, Name = "Theirs", SerialNumber = "2", Model = "M", AssignedUserId = "u2", Category = cat, CategoryId = 1 });

            var mine = (await _service.GetAssetsForUserAsync("u1")).ToList();

            Assert.That(mine, Has.Count.EqualTo(1));
            Assert.That(mine[0].Name, Is.EqualTo("Mine"));
        }

        [Test]
        public async Task GetDetailsAsync_WhenMissing_ReturnsNull()
        {
            var result = await _service.GetDetailsAsync(404);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAssetsAsync_FiltersBySearchTerm()
        {
            var cat = new Category { Id = 1, Name = "Laptops" };
            _data.Add(new Asset { Id = 1, Name = "Alpha", SerialNumber = "AA-1", Model = "M", Category = cat, CategoryId = 1 });
            _data.Add(new Asset { Id = 2, Name = "Beta", SerialNumber = "BB-2", Model = "M", Category = cat, CategoryId = 1 });

            var result = await _service.GetAssetsAsync("Alpha", null, 1, 10);

            Assert.That(result.Assets.TotalCount, Is.EqualTo(1));
            Assert.That(result.Assets.Items[0].Name, Is.EqualTo("Alpha"));
        }
    }
}
