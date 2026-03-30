using BLL.DTO;

namespace BLL.Interfaces
{
    public interface INewsService
    {
        Task<bool> AddNews(NewsDTO newsDTO);
        Task<bool> EditNews(NewsDTO newsDTO);
        Task<bool> DeleteNews(int newsId);

        Task<NewsDTO?> GetById(int id);
        Task<List<NewsDTO>> GetAll();
        Task<List<NewsDTO>> GetByCategory(int categoryId);
        Task<List<NewsDTO>> GetSortedByDate(bool descending = true, int page = 1, int pageSize = 20);
        Task<List<NewsDTO>> GetPopular(int minViews);

        Task<int> GetTotalCount();
    }
}