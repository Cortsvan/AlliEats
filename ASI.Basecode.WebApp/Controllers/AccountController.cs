using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
        }

        /// <summary>
        /// Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            this._session.SetString("HasSession", "Exist");

            User user = null;

            // Authenticate user against the database
            var loginResult = _userService.AuthenticateUser(model.UserId, model.Password, ref user);

            if (loginResult == LoginResult.Success && user != null)
            {
                // Authentication successful
                await this._signInManager.SignInAsync(user);

                // ✅ FIXED: Store both the UserId (email) AND UserName (display name)
                this._session.SetString("UserId", user.UserId);        // Store EMAIL for profile lookup
                this._session.SetString("UserName", user.Name);        // Store NAME for display
                this._session.SetString("UserRole", user.Role ?? "User");

                // Role-based redirection
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            } else if (loginResult == LoginResult.EmailNotVerified)
            {
                TempData["ErrorMessage"] = "Please verify your email address before logging in.";
                TempData["UnverifiedEmail"] = model.UserId;
                return RedirectToAction("VerifyEmail", new { email = model.UserId });
            }
            else
            {
                // Authentication failed
                TempData["ErrorMessage"] = "Incorrect UserId or Password";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var emailSent = await _userService.AddUserAsync(model);
                if (emailSent)
                {
                    TempData["SuccessMessage"] = "Account created successfully! Please check your email for the OTP verification code.";
                    return RedirectToAction("VerifyEmail", new { email = model.UserId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Account created but email could not be sent. Please contact support.";
                    return View(model);
                }
            }
            catch(InvalidDataException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
            }
            return View(model);
        }

        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            var model = new EmailVerificationViewModel
            {
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult VerifyEmail(EmailVerificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isVerified = _userService.VerifyOtp(model.Email, model.Otp);

            if (isVerified)
            {
                TempData["SuccessMessage"] = "Email verified successfully! You can now login with your credentials.";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid or expired OTP. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Email is required." });
            }

            var success = await _userService.ResendOtpAsync(email);

            if (success)
            {
                return Json(new { success = true, message = "OTP resent successfully. Please check your email." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to resend OTP. Please try again later." });
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var emailSent = await _userService.SendPasswordResetOtpAsync(model.Email);
                if (emailSent)
                {
                    TempData["SuccessMessage"] = "Password reset code sent to your email. Please check your inbox.";
                    return RedirectToAction("ResetPassword", new { email = model.Email });
                }
                else
                {
                    TempData["ErrorMessage"] = "Email not found or not verified. Please check your email address.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred. Please try again later.";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isReset = _userService.ResetPassword(model.Email, model.Otp, model.NewPassword);

            if (isReset)
            {
                TempData["SuccessMessage"] = "Password reset successfully! You can now login with your new password.";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid or expired verification code. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResendPasswordResetOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Email is required." });
            }

            var success = await _userService.SendPasswordResetOtpAsync(email);

            if (success)
            {
                return Json(new { success = true, message = "Password reset code resent successfully. Please check your email." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to resend code. Please try again later." });
            }
        }
    }
}
