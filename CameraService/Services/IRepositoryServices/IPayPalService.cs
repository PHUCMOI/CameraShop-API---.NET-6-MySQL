using CameraCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static CameraService.Services.PayPalService;

namespace CameraService.Services.IRepositoryServices
{
    public interface IPayPalService
    {
        Task<PayPalPayment> CreatePaymentUrl(PaymentInformationModel model);
    }
}
