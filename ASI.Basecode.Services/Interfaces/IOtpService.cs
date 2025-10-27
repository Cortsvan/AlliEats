using System;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task<bool> SendOtpEmailAsync(string email, string name, string otp);
        Task<bool> SendPasswordResetEmailAsync(string email, string name, string otp);
        bool ValidateOtp(string providedOtp, string storedOtp, DateTime? expiry);
    }
}