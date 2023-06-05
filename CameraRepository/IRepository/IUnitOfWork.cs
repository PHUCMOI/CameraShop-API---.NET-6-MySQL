using CameraCore.IRepository;

namespace CameraAPI.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICameraRepository Cameras{ get; }
        ICategoryRepository Categories { get; }
        IOrderRepository Orders { get; }
        IUserRepository Users { get; }
        IOrderDetailsRepository OrderDetails { get; }
        IWarehouseCameraRepository WarehouseCamera { get; }
        IWarehouseCategoryRepository WarehouseCategory { get; }
        int Save();
    }
}
