using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace WebCinema.Utilities.DbInitialization
{
    public class DbInitializer : IDbInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DbInitializer(RoleManager<IdentityRole> roleManager, 
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task Initialize()
        {
            if (_context.Database.GetPendingMigrations().Any())
            {
                _context.Database.Migrate();
            }

            if (_roleManager.Roles.IsNullOrEmpty())
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.SUPER_ADMIN_ROLE));
                await _roleManager.CreateAsync(new IdentityRole(SD.ADMIN_ROLE));
                await _roleManager.CreateAsync(new IdentityRole(SD.EMPLOYEE_ROLE));
                await _roleManager.CreateAsync(new IdentityRole(SD.CUSTOMER_ROLE));


                await _userManager.CreateAsync(new ApplicationUser()
                {
                    FName = "Super",
                    LName = "Admin",
                    Email = "superadmin@eraasoft.com",
                    EmailConfirmed = true,
                    UserName = "SuperAdmin",
                }, password: "SuperAdmin@123");

                await _userManager.CreateAsync(new ApplicationUser()
                {
                    FName = "Admin",
                    LName = "",
                    Email = "admin@eraasoft.com",
                    EmailConfirmed = true,
                    UserName = "Admin",
                }, password: "Admin@123");

                await _userManager.CreateAsync(new ApplicationUser()
                {
                    FName = "Employee",
                    LName = "1",
                    Email = "employee1@eraasoft.com",
                    EmailConfirmed = true,
                    UserName = "Employee1",
                }, password: "Employee@123");

                await _userManager.CreateAsync(new ApplicationUser()
                {
                    FName = "Employee",
                    LName = "2",
                    Email = "employee2@eraasoft.com",
                    EmailConfirmed = true,
                    UserName = "Employee2",
                }, password: "Employee@123");

                var user1 = await _userManager.FindByNameAsync("SuperAdmin");
                var user2 = await _userManager.FindByNameAsync("Admin");
                var user3 = await _userManager.FindByNameAsync("Employee1");
                var user4 = await _userManager.FindByNameAsync("Employee2");

                if (user1 is not null && user2 is not null && user3 is not null && user4 is not null)
                {
                    await _userManager.AddToRoleAsync(user1, SD.SUPER_ADMIN_ROLE);
                    await _userManager.AddToRoleAsync(user2, SD.ADMIN_ROLE);
                    await _userManager.AddToRoleAsync(user3, SD.EMPLOYEE_ROLE);
                    await _userManager.AddToRoleAsync(user4, SD.EMPLOYEE_ROLE);
                }
            }
        }
    }
}
