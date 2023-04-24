using CameraAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CameraAPI.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetAllWarehouse();
        Task<T> GetById(int id);
        Task Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
