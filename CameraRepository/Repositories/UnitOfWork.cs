using CameraAPI.Models;
using CameraCore.IRepository;

namespace CameraAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CameraAPIdbContext _context;
        private readonly WarehouseDbContext _warehouseDbContext;
        public ICameraRepository Cameras { get; }
        public ICategoryRepository Categories { get; }
        public IOrderRepository Orders {  get; }
        public IUserRepository Users { get; }
        public IOrderDetailsRepository OrderDetails { get; }
        public IWarehouseCameraRepository WarehouseCamera { get; }
        public IWarehouseCategoryRepository WarehouseCategory { get; }

        public UnitOfWork(CameraAPIdbContext context, WarehouseDbContext warehouseDbContext,
            ICameraRepository cameraRepository, ICategoryRepository categoryRepository, IOrderDetailsRepository orderDetailsRepository)
        {
            _context = context;
            _warehouseDbContext = warehouseDbContext;
            Cameras = cameraRepository;
            OrderDetails = orderDetailsRepository;
        }

        public int Save()
        {
            return _context.SaveChanges();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
    }
}
