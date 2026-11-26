using LushThreads.Application.Interfaces;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Application.Services;
using LushThreads.Domain.Entites;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace LushThreads.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
        private readonly Mock<IAdminActivityService> _activityMock = new();
        private readonly Mock<ILogger<CategoryService>> _loggerMock = new();
        private readonly CategoryService _sut;

        public CategoryServiceTests()
        {
            _sut = new CategoryService(_categoryRepoMock.Object, _activityMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsAllCategories()
        {
            var expected = new List<Category>
            {
                new() { Category_Id = 1, Category_Name = "Men" },
                new() { Category_Id = 2, Category_Name = "Women" }
            };

            _categoryRepoMock
                .Setup(r => r.GetAllAsync(null, null, false, null, null, null, default))
                .ReturnsAsync(expected);

            var result = (await _sut.GetAllCategoriesAsync()).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsCategory_WhenExists()
        {
            var category = new Category { Category_Id = 10, Category_Name = "Hoodies" };

            _categoryRepoMock
                .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Category, bool>>>(), null, false, default))
                .ReturnsAsync(category);

            var result = await _sut.GetCategoryByIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal("Hoodies", result!.Category_Name);
        }

        [Fact]
        public async Task GetMainCategoriesAsync_CallsRepositoryWithFilter()
        {
            var expected = new List<Category>
            {
                new() { Category_Id = 1, Category_Name = "Men", ParentCategoryId = null }
            };

            _categoryRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<Expression<Func<Category, bool>>>(), null, false, null, null, null, default))
                .ReturnsAsync(expected);

            var result = (await _sut.GetMainCategoriesAsync()).ToList();

            Assert.Single(result);
            Assert.Null(result[0].ParentCategoryId);
        }
    }
}
