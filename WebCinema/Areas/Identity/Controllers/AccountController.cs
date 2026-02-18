using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WebCinema.Repositories;
using WebCinema.Services;
using WebCinema.Services.IServices;

namespace WebCinema.Areas.Identity.Controllers
{
    [Area(SD.IDENTITY_AREA)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IAccountService _accountService;
        private readonly Repository<ApplicationUserOTP> _applicationUserOTPrepository;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IAccountService accountService,
            IRepository<ApplicationUserOTP> applicationUserOTPrepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _accountService = accountService;
            _applicationUserOTPrepository = (Repository<ApplicationUserOTP>?)applicationUserOTPrepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            ApplicationUser applicationUser = new ApplicationUser
            {
                FName = registerVM.FName,
                LName = registerVM.LName,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
                Address = registerVM.Address
            };

            //var applicationUser = registerVM.Adapt<ApplicationUser>();
            //applicationUser.Id = Guid.NewGuid().ToString();

            var result = await _userManager.CreateAsync(applicationUser, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
                return View(registerVM);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { area = "Identity", token, applicationUser.Id }, Request.Scheme);
            //await _emailSender.SendEmailAsync(applicationUser.Email, "Confirm your Account!", $"<h1>Click <a href='{confirmationLink}'>here</a> to confirm your account.</h1>");

            await _accountService.SendEmailAsync(EmailType.Confirmation, $"<h1>Click <a href='{confirmationLink}'>here</a> to confirm your account.</h1>", applicationUser);

            TempData["success-notification"] = "Registered successfully";

            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null) return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }

                TempData["error-notification"] = $"Invalid confirmation link, please try again";
            }
            else
            {
                TempData["success-notification"] = $"Confirm Account Successfully";
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _userManager.FindByEmailAsync(loginVM.EmailOrUserName) ?? await _userManager.FindByNameAsync(loginVM.EmailOrUserName);

            if (user is null)
            {
                ModelState.AddModelError("EmailOrUserName", "Invalid User Name Or Email.");
                ModelState.AddModelError("Password", "Invalid Password");

                return View(loginVM);
            }

            //var result = await _userManager.CheckPasswordAsync(user, loginVM.Password);
            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError("EmailOrUserName", "Confirm your email first!!");

                    return View(loginVM);
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("EmailOrUserName", "Too many attempt, please try again later");

                    return View(loginVM);
                }

                ModelState.AddModelError("EmailOrUserName", "Invalid User Name Or Email.");
                ModelState.AddModelError("Password", "Invalid Password");

                return View(loginVM);
            }


            TempData["success-notification"] = $"Login successfully, Welcome back {user.UserName}";

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
                return View(resendEmailConfirmationVM);

            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationVM.EmailOrUserName) ?? await _userManager.FindByNameAsync(resendEmailConfirmationVM.EmailOrUserName);

            if (user is not null && !user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new { area = "Identity", token, user.Id }, Request.Scheme);
                await _accountService.SendEmailAsync(EmailType.ResendConfirmation, $"<h1>Click <a href='{confirmationLink}'>here</a> to confirm your account.</h1>", user);

                //await _accountService.SendEmailAsync(user, Url, Request);
            }
            TempData["success-notification"] = $"Resend Email Successfully";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(forgetPasswordVM);

            var user = await _userManager.FindByEmailAsync(forgetPasswordVM.EmailOrUserName) ?? await _userManager.FindByNameAsync(forgetPasswordVM.EmailOrUserName);

            var userOtpsCount = (await _applicationUserOTPrepository.GetAsync(e => e.ApplicationUserId == user.Id && e.CreatedAt >= DateTime.UtcNow.AddHours(-24))).Count();

            if (!user.EmailConfirmed)
            {
                return RedirectToAction("ResendEmailConfirmation", "Account", new { area = "Identity" });
            }
            else if (user is not null && userOtpsCount <= 5)
            {
                string otp = new Random().Next(1000, 9999).ToString();

                string msg = $"<h1>OTP: {otp}. Don't share it.</h1>";

                await _accountService.SendEmailAsync(EmailType.ForgetPassword, msg, user);

                await _applicationUserOTPrepository.CreateAsync(new ApplicationUserOTP()
                {
                    ApplicationUserId = user.Id,
                    OTP = otp,
                });

                await _applicationUserOTPrepository.CommitAsync();

                TempData["success-notification"] = $"Send OTP to your Email Successfully";
            }
            else if (userOtpsCount > 5)
            {
                TempData["error-notification"] = $"Too Many Attemps!, Please Try Again after 24 hours";
            }

            return RedirectToAction("ValidateOTP", "Account", new { area = "Identity", applicationUserId = user.Id });
        }

        [HttpGet]
        public IActionResult ValidateOTP(string applicationUserId)
        {
            return View(new ValidateOTPVM
            {
                ApplicationUserId = applicationUserId,
            });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTPVM validateOTPVM)
        {
            if (!ModelState.IsValid)
            {
                return View(validateOTPVM);
            }
            var user = await _userManager.FindByIdAsync(validateOTPVM.ApplicationUserId);
            if (user is null) return NotFound();

            var otp = (await _applicationUserOTPrepository.GetAsync()).Where(e => e.ApplicationUserId == user.Id && e.IsValid).OrderBy(e => e.Id).LastOrDefault();

            if (otp == null)
            {
                TempData["error-notification"] = $"Invalid OTP, Please Try Again";
                return View(validateOTPVM);
            }
            otp.IsUsed = true;
            return RedirectToAction("ResetPassword", "Account", new { area = "Identity", applicationUserId = user.Id });
        }

        [HttpGet]
        public IActionResult ResetPassword(string applicationUserId)
        {
            return View(new ResetPasswordVM
            {
                ApplicationUserId = applicationUserId,
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(resetPasswordVM);

            var user = await _userManager.FindByIdAsync(resetPasswordVM.ApplicationUserId);
            if (user is null) return NotFound();

            var userToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, userToken, resetPasswordVM.Password);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("Password", string.Join(", ", result.Errors.Select(e=>e.Description)));
                return View(resetPasswordVM);
            }

            TempData["success-notification"] = $"Change Password Successfully";
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
