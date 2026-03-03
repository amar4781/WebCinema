using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebCinema.Areas.Admin.Controllers
{
    [Area(SD.ADMIN_AREA)]
    [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? fname, int page = 1)
        {
            var query = _userManager.Users.AsQueryable();
            if (fname is not null)
            {
                query = query.Where(c => c.FName.Contains(fname));
            }

            if (page < 1)
                page = 1;

            int pageSize = 5;
            int totalCount = query.Count();
            double totalPages = Math.Ceiling(totalCount / (double)pageSize);
            var users = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var usersWithRoles = new List<UserWithRolesVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                usersWithRoles.Add(new UserWithRolesVM
                {
                    User = user,
                    Roles = roles
                });
            }


            return View(new UsersVM
            {
                ApplicationUsers = usersWithRoles,
                CurrentPage = page,
                TotalPages = totalPages
            });
        }

        [HttpGet]
        public IActionResult Create()
        {
            var roles = _roleManager.Roles.Select(r => r.Name!).ToList();

            var vm = new CreateUserVM
            {
                Roles = roles
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Roles = _roleManager.Roles.Select(r => r.Name!).ToList();
                return View(vm);
            }
            var user = new ApplicationUser
            {
                FName = vm.FName,
                LName = vm.LName,
                Email = vm.Email,
                UserName = vm.UserName
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                vm.Roles = _roleManager.Roles.Select(r => r.Name!).ToList();
                return View(vm);
            }

            await _userManager.AddToRoleAsync(user, vm.SelectedRole);

            TempData["success-notification"] = "User created successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var roles = _roleManager.Roles.Select(r => r.Name!).ToList();

            var vm = new EditUserVM
            {
                Id = user.Id,
                FName = user.FName,
                LName = user.LName,
                Email = user.Email,
                Roles = roles,
                SelectedRole = userRoles.FirstOrDefault()!
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Roles = _roleManager.Roles.Select(r => r.Name!).ToList();
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null) return NotFound();

            user.FName = vm.FName;
            user.LName = vm.LName;
            user.Email = vm.Email;
            user.UserName = vm.Email;

            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, vm.SelectedRole);

            TempData["success-notification"] = "User updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (id == currentUserId)
            {
                TempData["error-notification"] = "You cannot delete yourself.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(SD.SUPER_ADMIN_ROLE))
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync(SD.SUPER_ADMIN_ROLE);

                if (superAdmins.Count <= 1)
                {
                    TempData["error-notification"] = "Cannot delete the last Super Admin.";
                    return RedirectToAction(nameof(Index));
                }
            }

            await _userManager.DeleteAsync(user);

            TempData["success-notification"] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
