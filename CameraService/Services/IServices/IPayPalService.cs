using CameraCore.Models;
using static CameraService.Services.PayPalService;

namespace CameraService.Services.IRepositoryServices
{
    public interface IPayPalService
    {
        Task<PayPalPayment> CreatePaymentUrl(PaymentInformationModel model);
    }
}
