﻿using CameraAPI.AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraCore.IRepository
{
    public interface ILoginRepository
    {
        UserModel CheckLogin(UserModel _userData);
    }
}
