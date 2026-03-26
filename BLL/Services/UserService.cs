using AutoMapper;
using BLL.DTO;
using BLL.Interfaces;
using DAL;
using DAL.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IRepository<User> _repository;

        public UserService(IMapper mapper, IRepository<User> repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<bool> AddUser(UserDTO userDTO)
        {
            if (await FindUserByUsername(userDTO.Username) != null)
                throw new ValidationException("Такий користувач вже існує");

            var user = _mapper.Map<User>(userDTO);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
            await _repository.Add(user);
            await _repository.SaveChanges();

            return true;
        }
        public async Task<UserDTO?> Authenticate(string username, string password)
        {
            var user = await _repository.GetAll()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            return isPasswordValid ? _mapper.Map<UserDTO>(user) : null;
        }
        public async Task<UserDTO?> FindUserByUsername(string? username)
        {
            var user = await _repository.GetAll()
                .FirstOrDefaultAsync(c => c.Username == username);

            return user == null ? null : _mapper.Map<UserDTO>(user);
        }
        public async Task<bool> UpdateUser(UserDTO userDTO)
        {
            var user = await _repository.Find(u => u.Username == userDTO.Username);
            if (user == null)
                throw new Exception("Користувача не знайдено");

            if (!string.IsNullOrEmpty(userDTO.Username))
                user.Username = userDTO.Username;

            if (!string.IsNullOrEmpty(userDTO.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);

            await _repository.Update(user);
            await _repository.SaveChanges();

            return true;
        }

        public async Task<bool> DeleteUser(UserDTO userDTO)
        {
            var user = await _repository.Find(u => u.Username == userDTO.Username);

            if (user == null)
                throw new Exception("Неможливо видалити користувача, оскільки його не знайдено в базі даних");

            await _repository.Remove(user);
            await _repository.SaveChanges();

            return true;
        }
    }
}