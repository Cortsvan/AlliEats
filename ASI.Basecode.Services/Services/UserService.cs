using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly IOtpService _otpService;

        public UserService(IUserRepository repository, IMapper mapper, IOtpService otpService)
        {
            _mapper = mapper;
            _repository = repository;
            _otpService = otpService;
        }

        public LoginResult AuthenticateUser(string userId, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.UserId == userId &&
                                                     x.Password == passwordKey).FirstOrDefault();

            if (user != null && !user.IsEmailVerified)
            {
                return LoginResult.EmailNotVerified;
            }

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }
        
        public IQueryable<User> GetAllUsers()
        {
            return _repository.GetUsers();
        }
        
        public async Task<bool> AddUserAsync(UserViewModel model)
        {
            var user = new User();
            if (!_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.Role = "User";
                user.IsEmailVerified = false;
                user.EmailVerificationToken = _otpService.GenerateOtp();
                user.EmailVerificationTokenExpiry = DateTime.Now.AddMinutes(15); // 15 minutes expiry
                user.CreatedTime = DateTime.Now;
                user.UpdatedTime = DateTime.Now;
                user.CreatedBy = System.Environment.UserName;
                user.UpdatedBy = System.Environment.UserName;

                _repository.AddUser(user);

                // Send OTP email
                var emailSent = await _otpService.SendOtpEmailAsync(user.UserId, user.Name, user.EmailVerificationToken);
                return emailSent;
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        public void AddUser(UserViewModel model)
        {
            var user = new User();
            if (!_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.Role = "User"; // Default role for regular users
                user.IsEmailVerified = true;
                user.CreatedTime = DateTime.Now;
                user.UpdatedTime = DateTime.Now;
                user.CreatedBy = System.Environment.UserName;
                user.UpdatedBy = System.Environment.UserName;

                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }
        public bool VerifyOtp(string email, string otp)
        {
            var user = _repository.GetUserById(email);
            if (user == null) return false;

            var isValid = _otpService.ValidateOtp(otp, user.EmailVerificationToken, user.EmailVerificationTokenExpiry);

            if (isValid)
            {
                _repository.UpdateUserVerification(email, true);
            }

            return isValid;
        }

        public async Task<bool> ResendOtpAsync(string email)
        {
            var user = _repository.GetUserById(email);
            if (user == null || user.IsEmailVerified) return false;

            var newOtp = _otpService.GenerateOtp();
            user.EmailVerificationToken = newOtp;
            user.EmailVerificationTokenExpiry = DateTime.Now.AddMinutes(15);
            _repository.UpdateUser(user);

            return await _otpService.SendOtpEmailAsync(user.UserId, user.Name, newOtp);
        }

        public void CreateDefaultAdmin()
        {
            const string adminEmail = "allieatsadmin@gmail.com";
            const string adminPassword = "@Admin123";
            const string adminName = "AlliEats Administrator";

            if (!_repository.UserExists(adminEmail))
            {
                var adminUser = new User
                {
                    UserId = adminEmail,
                    Name = adminName,
                    Password = PasswordManager.EncryptPassword(adminPassword),
                    Role = "Admin",
                    IsEmailVerified = true,
                    CreatedTime = DateTime.Now,
                    UpdatedTime = DateTime.Now,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                };

                _repository.AddUser(adminUser);
            }
        }
        public async Task<bool> SendPasswordResetOtpAsync(string email)
        {
            var user = _repository.GetUserById(email);
            if (user == null || !user.IsEmailVerified)
            {
                return false;
            }
            var newOtp = _otpService.GenerateOtp();
            user.EmailVerificationToken = newOtp;
            user.EmailVerificationTokenExpiry = DateTime.Now.AddMinutes(15);
            _repository.UpdateUser(user);

            return await _otpService.SendPasswordResetEmailAsync(user.UserId, user.Name, newOtp);
        }
        public bool VerifyPasswordResetOtp(string email, string otp)
        {
            var user = _repository.GetUserById(email);
            if (user == null || !user.IsEmailVerified) return false;

            return _otpService.ValidateOtp(otp, user.EmailVerificationToken, user.EmailVerificationTokenExpiry);
        }
        public bool ResetPassword(string email, string otp, string newPassword)
        {
            var user = _repository.GetUserById(email);
            if (user == null || !user.IsEmailVerified) return false;

            // Verify OTP first
            var isValidOtp = _otpService.ValidateOtp(otp, user.EmailVerificationToken, user.EmailVerificationTokenExpiry);
            if (!isValidOtp) return false;

            // Update password
            user.Password = PasswordManager.EncryptPassword(newPassword);
            user.EmailVerificationToken = null; // Clear the OTP
            user.EmailVerificationTokenExpiry = null;
            user.UpdatedTime = DateTime.Now;
            user.UpdatedBy = user.UserId;

            _repository.UpdateUser(user);
            return true;
        }
        public bool ChangePassword(string email, string currentPassword, string newPassword)
        {
            try
            {
                var user = _repository.GetUserById(email);
                if (user == null || !user.IsEmailVerified) return false;

                // Verify current password
                var encryptedCurrentPassword = PasswordManager.EncryptPassword(currentPassword);
                if (user.Password != encryptedCurrentPassword)
                {
                    return false;
                }

                // Update to new password
                user.Password = PasswordManager.EncryptPassword(newPassword);
                user.UpdatedTime = DateTime.Now;
                user.UpdatedBy = user.UserId;

                _repository.UpdateUser(user);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public (bool IsValid, string Message) ValidatePasswordChange(string email, string currentPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return (false, "Email is required.");
                }

                if (string.IsNullOrEmpty(currentPassword))
                {
                    return (false, "Current password is required.");
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    return (false, "New password is required.");
                }

                if (newPassword.Length < 8)
                {
                    return (false, "New password must be at least 8 characters long.");
                }

                if (currentPassword == newPassword)
                {
                    return (false, "New password must be different from the current password.");
                }

                var user = _repository.GetUserById(email);
                if (user == null)
                {
                    return (false, "User not found.");
                }

                if (!user.IsEmailVerified)
                {
                    return (false, "Email is not verified.");
                }

                // Verify current password
                var encryptedCurrentPassword = PasswordManager.EncryptPassword(currentPassword);
                if (user.Password != encryptedCurrentPassword)
                {
                    return (false, "Current password is incorrect.");
                }

                return (true, "Password change is valid.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating password change: {ex.Message}", ex);
            }
        }

        public bool ValidateUserSession(string sessionUserId, string requestUserId)
        {
            return !string.IsNullOrEmpty(sessionUserId) &&
                   !string.IsNullOrEmpty(requestUserId) &&
                   sessionUserId == requestUserId;
        }
    }
}
