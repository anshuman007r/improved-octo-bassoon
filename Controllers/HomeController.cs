using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp_for_deployment.Models;

namespace WebApp_for_deployment.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;

        public HomeController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }
        [HttpGet]
        public  IActionResult Index()
        {
            string userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!(User.IsInRole("Admin")))
            {
                return RedirectToAction("Index", "Contacts");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> Index(CreateRole model)
        {
            if (ModelState.IsValid)
            {

               IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };
                IdentityResult result = await roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListView", "Home");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public  IActionResult ListView()
        {
            //var roles = roleManager.Roles;
            //var role = await roleManager.FindByNameAsync("Admin");
            //var extractedUserId = role.Name;
            var model = new List<UserRole>();
            foreach (var user in userManager.Users)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                };
                model.Add(userRole);
            }
            //var users = userManager.Users;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ListView(List<UserRole> userRole)
        {
            //var RoleName = "Admin";
            //var role = await roleManager.FindByNameAsync(RoleName);
            //var extractedUserId = role.Id;
            //return Json(extractedUserId);
            for (int i = 0; i < userRole.Count; i++)
            {
                var user = await userManager.FindByIdAsync(userRole[i].UserId);
                IdentityResult result = null;
                IdentityResult negresult = null;
                if (userRole[i].RoleName != null && !(await userManager.IsInRoleAsync(user, "Acceptor")) && !(await userManager.IsInRoleAsync(user, "User")))
                {
                    result = await userManager.AddToRoleAsync(user, userRole[i].RoleName);
                }
                else if (await userManager.IsInRoleAsync(user, "Acceptor"))
                {
                    result = await userManager.RemoveFromRoleAsync(user, "Acceptor");
                    negresult = await userManager.AddToRoleAsync(user, "User");
                }
                else if (await userManager.IsInRoleAsync(user, "User"))
                {
                    result = await userManager.RemoveFromRoleAsync(user, "User");
                    negresult = await userManager.AddToRoleAsync(user, "Acceptor");
                }
                else
                {
                    continue;
                }
                if (result.Succeeded || negresult.Succeeded)
                {
                    if (i == userRole.Count - 1)
                    {
                        return RedirectToAction("Index", "Contacts");
                    }

                }
            }
            return View(userRole);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
