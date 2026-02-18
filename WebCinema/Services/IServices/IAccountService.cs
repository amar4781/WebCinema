using Microsoft.AspNetCore.Mvc;

namespace WebCinema.Services.IServices
{
    public interface IAccountService
    {
        Task SendEmailAsync(EmailType emailType, string msg, ApplicationUser applicationUser);
    }
}
