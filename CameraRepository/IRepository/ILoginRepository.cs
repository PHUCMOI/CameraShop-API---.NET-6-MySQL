using CameraAPI.AppModel;

namespace CameraCore.IRepository
{
    public interface ILoginRepository
    {
        UserModel CheckLogin(string name, string password);
        Boolean checkRefreshToken(string refreshtoken);

    }
}
