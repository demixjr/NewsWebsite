using AutoMapper;
using BLL.DTO;
using BLL.Interfaces;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BLL.Services
{
    public class NewsService : INewsService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<News> _repository;
        public NewsService(IMapper mapper, IRepository<News> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<bool> AddNews(NewsDTO newsDTO)
        {
            var news = _mapper.Map<News>(newsDTO);
            news.Date = DateTime.UtcNow;
            news.Views = 0;

            await _repository.Add(news);
            await _repository.SaveChanges();

            return true;
        }

        public async Task<bool> EditNews(NewsDTO newsDTO)
        {
            var news = await _repository.Find(n => n.Id == newsDTO.Id);
            if (news == null)
                throw new KeyNotFoundException("Новину не знайдено");

            news.Title = newsDTO.Title;
            news.Description = newsDTO.Description;
            news.CategoryId = newsDTO.CategoryId;

            await _repository.Update(news);
            await _repository.SaveChanges();

            return true;
        }

        public async Task<bool> DeleteNews(int newsId)
        {
            var news = await _repository.Find(n => n.Id == newsId);
            if (news == null)
                throw new Exception("Новину не знайдено");

            await _repository.Remove(news);
            await _repository.SaveChanges();

            return true;
        }

        public async Task<NewsDTO?> GetById(int id)
        {
            var news = await _repository.Find(n => n.Id == id);
            if (news == null) return null;

            news.Views++;
            await _repository.Update(news);
            await _repository.SaveChanges();

            return _mapper.Map<NewsDTO>(news);
        }

        public async Task<List<NewsDTO>> GetAll()
        {
            var news = await _repository.GetAll().ToListAsync();
            return _mapper.Map<List<NewsDTO>>(news);
        }

        public async Task<List<NewsDTO>> GetByCategory(int categoryId)
        {
            var news = await _repository.GetAll()
                .Include(n => n.Category) 
                .Include(n => n.Author)
                .Where(n => n.CategoryId == categoryId)
                .ToListAsync();

            return _mapper.Map<List<NewsDTO>>(news);
        }


        public async Task<List<NewsDTO>> GetSortedByDate(bool descending = true, int page = 1, int pageSize = 20)
        {
            var query = _repository.GetAll()
                .Include(n => n.Category)
                .Include(n => n.Author);

            var orderedQuery = descending
                ? query.OrderByDescending(n => n.Date)
                : query.OrderBy(n => n.Date);

            var news = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<NewsDTO>>(news);
        }

        public async Task<List<NewsDTO>> GetPopular(int minViews)
        {
            var news = await _repository.GetAll()
                .Where(n => n.Views >= minViews)
                .OrderByDescending(n => n.Views)
                .ToListAsync();

            return _mapper.Map<List<NewsDTO>>(news);
        }

        public async Task<int> GetTotalCount()
        {
            return await _repository.GetAll().CountAsync();
        }
    }
}