using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // If user is logged in, redirect to their dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Faculty"))
                    {
                        return RedirectToAction("Dashboard", "Faculty");
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Student"))
                    {
                        return RedirectToAction("Dashboard", "Students");
                    }
                }
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature?.Error != null)
            {
                _logger.LogError(exceptionFeature.Error,
                    "Unhandled exception on path {Path} by user {User}",
                    exceptionFeature.Path,
                    User.Identity?.Name ?? "Anonymous");
            }

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("Access denied attempt by user {User} on path {Path}",
                User.Identity?.Name ?? "Unknown",
                HttpContext.Request.Path);
            return View();
        }
    }
}