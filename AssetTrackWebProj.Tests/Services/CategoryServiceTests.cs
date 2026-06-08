using AssetTrack.Data.Models;
using AssetTrack.Services.Exceptions;
using AssetTrack.Services.Implementations;
using AssetTrack.Services.Models.Categories;
using AssetTrack.Services.Repository;
using MockQueryable;

namespace AssetTrack.Tests.Services
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private Mock<IRepository<Category>> _repo = null!;
        private List<Category> _data = null!;
        private CategoryService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _data = new List<Category>();
            _repo = new Mock<IRepository<Category>>();
            _repo.Setup(r => r.AllAsNoTracking()).Returns(() => _data.BuildMock());
            _repo.Setup(r => r.All()).Returns(() => _data.BuildMock());
            _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _repo.Setup(r => r.AddAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask)
                .Callback<Category>(c =>
                {
                    if (c.Id == 0) c.Id = _data.Count + 1;
                    _data.Add(c);
                });

            _service = new CategoryService(_repo.Object);
        }

        [Test]
        public async Task CreateAsync_WithUniqueName_AddsCategory()
        {
            var id = await _service.CreateAsync(new CategoryFormModel { Name = "Networking" });

            Assert.That(id, Is.GreaterThan(0));
            Assert.That(_data, Has.Count.EqualTo(1));
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void CreateAsync_WithDuplicateName_Throws()
        {
            _data.Add(new Category { Id = 1, Name = "Laptops" });

            Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.CreateAsync(new CategoryFormModel { Name = "Laptops" }));
        }

        [Test]
        public void UpdateAsync_WhenNotFound_Throws()
        {
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync((Category?)null);

            Assert.ThrowsAsync<ServiceValidationException>(
                async () => await _service.UpdateAsync(new CategoryFormModel { Id = 9, Name = "X" }));
        }

        [Test]
        public async Task UpdateAsync_WithValidData_UpdatesCategory()
        {
            var existing = new Category { Id = 2, Name = "Monitors", Description = "old" };
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<object[]>())).ReturnsAsync(existing);

            await _service.UpdateAsync(new CategoryFormModel { Id = 2, Name = "Displays", Description = "new" });

            Assert.That(existing.Name, Is.EqualTo("Displays"));
            Assert.That(existing.Description, Is.EqualTo("new"));
            _repo.Verify(r => r.Update(existing), Times.Once);
        }

        [Test]
        public async Task GetAllAsync_ReturnsCategoriesOrderedByName()
        {
            _data.Add(new Category { Id = 1, Name = "Servers" });
            _data.Add(new Category { Id = 2, Name = "Laptops" });

            var result = (await _service.GetAllAsync()).ToList();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Laptops"));
        }

        [Test]
        public async Task GetCategoryOptionsAsync_ReturnsLightweightOptions()
        {
            _data.Add(new Category { Id = 1, Name = "Laptops" });

            var options = (await _service.GetCategoryOptionsAsync()).ToList();

            Assert.That(options, Has.Count.EqualTo(1));
            Assert.That(options[0].Id, Is.EqualTo(1));
            Assert.That(options[0].Name, Is.EqualTo("Laptops"));
        }
    }
}
