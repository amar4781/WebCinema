using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using WebCinema.Services.IServices;

namespace WebCinema.Services
{
    public enum EmailType
    {
        Confirmation,
        ResendConfirmation,
        ForgetPassword,
    }
    public class AccountService : IAccountService
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(IEmailSender emailSender,UserManager<ApplicationUser> userManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public async Task SendEmailAsync(EmailType emailType, string msg, ApplicationUser applicationUser)
        {
            if (emailType == EmailType.Confirmation)
            {
                await _emailSender.SendEmailAsync(applicationUser.Email!, "Confirm your Account!", msg);
            }
            else if (emailType == EmailType.ResendConfirmation)
            {
                await _emailSender.SendEmailAsync(applicationUser.Email!, "Resend Confirm your Account!", msg);
            }
            else if (emailType == EmailType.ForgetPassword)
            {
                await _emailSender.SendEmailAsync(applicationUser.Email!, "Forget Password!", msg);
            }
        }
    }
}
