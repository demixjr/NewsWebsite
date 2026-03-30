using AutoMapper;
using BLL;
using BLL.DTO;
using BLL.Services;
using DAL;
using DAL.Models;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace Tests.BLL.Tests
{
    public class NewsServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IRepository<News>> _repositoryMock;
        private readonly NewsService _newsService;

        public NewsServiceTests()
        {
            var expression = new MapperConfigurationExpression();
            expression.AddProfile<MappingProfile>();

            var config = new MapperConfiguration(expression);
            _mapper = config.CreateMapper();
            _repositoryMock = new Mock<IRepository<News>>();
            _newsService = new NewsService(_mapper, _repositoryMock.Object);
        }

        #region AddNews Tests

        [Fact]
        public async Task AddNews_ValidNews_ReturnsTrue()
        {
            // Arrange
            var newsDTO = new NewsDTO 
            { 
                Title = "Тестова новина",
                Description = "Опис новини",
                CategoryId = 1,
                AuthorId = 1
            };

            _repositoryMock.Setup(r => r.Add(It.IsAny<News>())).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _newsService.AddNews(newsDTO);

            // Assert
            Assert.True(result);
            _repositoryMock.Verify(r => r.Add(It.Is<News>(n => 
                n.Title == "Тестова новина" &&
                n.Views == 0 &&
                n.Date != default(DateTime)
            )), Times.Once);
            _repositoryMock.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task AddNews_SetsDateToUtcNow()
        {
            // Arrange
            var newsDTO = new NewsDTO 
            { 
                Title = "Test",
                Description = "Description"
            };

            _repositoryMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            News? capturedNews = null;
            _repositoryMock.Setup(r => r.Add(It.IsAny<News>()))
                .Callback<News>(n => capturedNews = n)
                .Returns(Task.CompletedTask);

            var beforeTime = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await _newsService.AddNews(newsDTO);

            var afterTime = DateTime.UtcNow.AddSeconds(1);

            // Assert
            Assert.NotNull(capturedNews);
            Assert.True(capturedNews.Date >= beforeTime && capturedNews.Date <= afterTime);
        }

        [Fact]
        public async Task AddNews_SetsViewsToZero()
        {
            // Arrange
            var newsDTO = new NewsDTO { Title = "Test" };
            _repositoryMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            News? capturedNews = null;
            _repositoryMock.Setup(r => r.Add(It.IsAny<News>()))
                .Callback<News>(n => capturedNews = n)
                .Returns(Task.CompletedTask);

            // Act
            await _newsService.AddNews(newsDTO);

            // Assert
            Assert.NotNull(capturedNews);
            Assert.Equal(0, capturedNews.Views);
        }

        #endregion

        #region EditNews Tests

        [Fact]
        public async Task EditNews_ExistingNews_ReturnsTrue()
        {
            // Arrange
            var newsDTO = new NewsDTO 
            { 
                Id = 1,
                Title = "Оновлена новина",
                Description = "Оновлений опис",
                CategoryId = 2
            };

            var existingNews = new News 
            { 
                Id = 1,
                Title = "Стара новина",
                Description = "Старий опис",
                CategoryId = 1,
                Date = DateTime.UtcNow.AddDays(-1),
                Views = 10
            };

            _repositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<News, bool>>>()))
                .ReturnsAsync(existingNews);
            _repositoryMock.Setup(r => r.Update(existingNews)).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _newsService.EditNews(newsDTO);

            // Assert
            Assert.True(result);
            Assert.Equal("Оновлена новина", existingNews.Title);
            Assert.Equal("Оновлений опис", existingNews.Description);
            Assert.Equal(2, existingNews.CategoryId);
            _repositoryMock.Verify(r => r.Update(existingNews), Times.Once);
            _repositoryMock.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task EditNews_NonExistentNews_ThrowsKeyNotFoundException()
        {
            // Arrange
            var newsDTO = new NewsDTO
            {
                Id = 999,
                Title = "Новина",
                Description = "Опис"
            };

            _repositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<News, bool>>>()))
                .ReturnsAsync((News?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _newsService.EditNews(newsDTO)
            );

            Assert.Equal("Новину не знайдено", exception.Message);
            _repositoryMock.Verify(r => r.Update(It.IsAny<News>()), Times.Never);
        }

        #endregion

        #region DeleteNews Tests

        [Fact]
        public async Task DeleteNews_ExistingNews_ReturnsTrue()
        {
            // Arrange
            var newsId = 1;
            var news = new News { Id = newsId, Title = "Новина для видалення" };

            _repositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<News, bool>>>()))
                .ReturnsAsync(news);
            _repositoryMock.Setup(r => r.Remove(news)).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _newsService.DeleteNews(newsId);

            // Assert
            Assert.True(result);
            _repositoryMock.Verify(r => r.Remove(news), Times.Once);
            _repositoryMock.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task DeleteNews_NonExistentNews_ThrowsException()
        {
            // Arrange
            var newsId = 999;

            _repositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<News, bool>>>()))
                .ReturnsAsync((News?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _newsService.DeleteNews(newsId)
            );

            Assert.Equal("Новину не знайдено", exception.Message);
            _repositoryMock.Verify(r => r.Remove(It.IsAny<News>()), Times.Never);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ExistingNews_ReturnsNewsDTOAndIncrementsViews()
        {
            // Arrange
            var newsId = 1;
            var news = new News 
            { 
                Id = newsId,
                Title = "Тестова новина",
                Views = 5
            };

            _repositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<News, bool>>>()))
                .ReturnsAsync(news);
            _repositoryMock.Setup(r => r.Update(news)).Returns(Task.CompletedTask);
            _repositoryMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _newsService.GetById(newsId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newsId, result.Id);
            Assert.Equal(6, news.Views); // Views incremented
            _repositoryMock.Verify(r => r.Update(news), Times.Once);
            _repositoryMock.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task GetById_NonExistentNews_ReturnsNull()
        {
            // Arrange
            var newsId = 999;

            _repositoryMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<News, bool>>>()))
                .ReturnsAsync((News?)null);

            // Act
            var result = await _newsService.GetById(newsId);

            // Assert
            Assert.Null(result);
            _repositoryMock.Verify(r => r.Update(It.IsAny<News>()), Times.Never);
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsAllNews()
        {
            // Arrange
            var newsList = new List<News>
            {
                new News { Id = 1, Title = "Новина 1" },
                new News { Id = 2, Title = "Новина 2" },
                new News { Id = 3, Title = "Новина 3" }
            }.AsQueryable().BuildMock();

            _repositoryMock.Setup(r => r.GetAll()).Returns(newsList);

            // Act
            var result = await _newsService.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var newsList = new List<News>().AsQueryable().BuildMock();
            _repositoryMock.Setup(r => r.GetAll()).Returns(newsList);

            // Act
            var result = await _newsService.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion
    }
}
