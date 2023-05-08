using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraAPI.Models;

namespace CameraAPI.Repositories
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly CameraAPIdbContext _context;
        protected readonly WarehouseDbContext _warehouseDbContext;

        protected GenericRepository(WarehouseDbContext dbContext)
        {
            _warehouseDbContext = dbContext;
        }

        protected GenericRepository(CameraAPIdbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllWarehouse()
        {
            return await _warehouseDbContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task Create(T entity)
        {
            await _context.Set<T>().AddAsync(entity);        
        } 

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
    }
}
