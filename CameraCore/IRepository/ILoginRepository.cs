using CameraAPI.AppModel;

namespace CameraCore.IRepository
{
    public interface ILoginRepository
    {
        UserModel CheckLogin(UserModel _userData);
    }
}
