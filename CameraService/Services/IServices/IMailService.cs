using AutoMapper.Internal;
using CameraCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraService.Services.IServices
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
