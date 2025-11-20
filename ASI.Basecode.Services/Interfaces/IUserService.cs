using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Linq;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string userid, string password, ref User user);
        void AddUser(UserViewModel model);
        Task<bool> AddUserAsync(UserViewModel model);
        bool VerifyOtp(string email, string otp);
        Task<bool> ResendOtpAsync(string email);
        void CreateDefaultAdmin();
        Task<bool> SendPasswordResetOtpAsync(string email);
        bool VerifyPasswordResetOtp(string email, string otp);
        bool ResetPassword(string email, string otp, string newPassword);
        bool ChangePassword(string email, string currentPassword, string newPassword);
        IQueryable<User> GetAllUsers();

        // validation methods
        (bool IsValid, string Message) ValidatePasswordChange(string email, string currentPassword, string newPassword);
        bool ValidateUserSession(string sessionUserId, string requestUserId);
    }
}