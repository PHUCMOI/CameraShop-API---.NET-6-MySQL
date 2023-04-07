using CameraAPI.Models;
using CameraCore.IRepository;

namespace CameraAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CameraAPIdbContext _context;
        public ICameraRepository Cameras { get; }
        public ICategoryRepository Categories { get; }
        public IOrderRepository Orders {  get; }
        public IUserRepository Users { get; }
        public IOrderDetailsRepository OrderDetails { get; }
        public UnitOfWork(CameraAPIdbContext context, ICameraRepository cameraRepository,
                           ICategoryRepository categoryRepository, IOrderDetailsRepository orderDetailsRepository)
        {
            _context = context;
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
