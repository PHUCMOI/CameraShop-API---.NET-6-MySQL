using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CameraService.Services.IRepositoryServices
{
    public interface IPayPalService
    {
        Task<string> CreatePaymentUrl(PaymentInformationModel model);
    }
}
