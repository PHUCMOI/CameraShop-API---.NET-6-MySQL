using CameraAPI.AppModel;
using CameraCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraService.Services.IServices
{
    public interface ILoginService
    {
        string Login(UserLogin userObj);
    }
}
