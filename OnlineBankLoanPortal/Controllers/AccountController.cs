using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineBankLoanPortal.Models;
using OnlineBankLoanPortal.ViewModels;

namespace OnlineBankLoanPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    //    return Redirect(returnUrl);

                    //else
                    //    return RedirectToAction("Index", "LoanApplication");
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    _logger.LogInformation("User logged in.");

                    // Get the user's roles
                    var roles = await _userManager.GetRolesAsync(user);

                    // Redirect based on role
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("LoanApplications", "Admin");
                    }
                    else if (roles.Contains("LoanOfficer"))
                    {
                        return RedirectToAction("LoanApplications", "LoanOfficer");
                    }
                    else if (roles.Contains("Customer"))
                    {
                        return RedirectToAction("LoanApplications", "Customer");
                    }
                    else
                    {
                        // Default
                        return LocalRedirect(returnUrl ?? "/");
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { FullName = model.Name, UserName = model.Email, Email = model.Email, EmailConfirmed = true, PhoneNumberConfirmed = true, PhoneNumber = model.Phone };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "LoanApplication");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}
