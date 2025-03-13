using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBankLoanPortal.Models;
using OnlineBankLoanPortal.Repository;
using OnlineBankLoanPortal.ViewModels;
using System.Security.Claims;

namespace OnlineBankLoanPortal.Controllers
{
    public class LoanApplicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoanApplicationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LoanApplication/Index
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Index()
        {
            List<LoanApplicationVM> applications = await GetUserLoanApplications();
            return View(applications);
        }

        // GET: LoanApplication/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var products = await _context.LoanProducts.ToListAsync();
            ViewBag.Products = products;

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var model = new LoanApplicationVM
            {
                UserId = userId,
                ApplicantName = user.FullName,
                ApplicantEmail = user.Email
            };
            return View(model);
        }

        // POST: LoanApplication/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoanApplicationVM model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Get the logged-in user's ID

                model.Status = "Pending";
                var application = new LoanApplication
                {
                    UserId = model.UserId,
                    ProductId = model.ProductId,
                    Status = model.Status,
                    DateApplied = model.DateApplied,
                    AmountRequested = model.AmountRequested
                };
                _context.LoanApplications.Add(application);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to a suitable page after submission
            }
            return View(model);
        }


        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Edit(int id)
        {
            var application = await _context.LoanApplications.FindAsync(id);
            if (application == null)
            {
                return NotFound();
            }

            // Map to ViewModel if needed
            // Return the view with the application data
            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LoanApplication application)
        {
            if (ModelState.IsValid)
            {
                _context.Update(application);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var application = await _context.LoanApplications
                    .Where(a => a.ApplicationId == id)
                    .Include(a => a.Repayments)
                    .FirstOrDefaultAsync();

            if (application == null)
                return NotFound();

            if (application.Status != "Pending" || application.Repayments.Any())
            {
                //ModelState.AddModelError("", "Cannot delete this application because it has associated repayments.");
                TempData["ErrorMessage"] = "Cannot delete application. Repayments have been initiated.";
                RedirectToAction(nameof(Index));
            }

            _context.LoanApplications.Remove(application);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<LoanApplicationVM>> GetUserLoanApplications()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var applications = await _context.LoanApplications
                .Where(a => a.UserId == userId)
                .Include(a => a.Product)
                .Include(a => a.ApplicationUser)
                .Select(a => new LoanApplicationVM
                {
                    Id = a.ApplicationId,
                    ProductName = a.Product.ProductName,
                    AmountRequested = a.AmountRequested,
                    ApplicantName = a.ApplicationUser.FullName,
                    Status = a.Status
                })
                .ToListAsync();
            return applications;
        }
    }
}
