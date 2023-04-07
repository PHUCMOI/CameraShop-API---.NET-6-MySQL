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

        int Save();
    }
}
