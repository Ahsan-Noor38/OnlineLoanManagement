using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBankLoanPortal.Models;
using OnlineBankLoanPortal.Repository;
using OnlineBankLoanPortal.ViewModels;

namespace OnlineBankLoanPortal.Controllers
{
    public class RepaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RepaymentController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Repayment/Create
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Index(int? applicationId)
        {
            var repayments = await _context.Repayments
                .Include(r => r.Application.Product)
                .Where(r => !applicationId.HasValue || r.ApplicationId == applicationId)
                .OrderByDescending(r => r.PaymentDate)
                .ToListAsync();

            return View(repayments);
        }

        // GET: Repayment/Create
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(int applicationId)
        {
            var model = new RepaymentVM
            {
                LoanApplicationId = applicationId
            };
            return View(model);
        }

        // POST: Repayment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RepaymentVM model)
        {
            if (ModelState.IsValid)
            {
                var loanApplicationExists = await _context.LoanApplications.AnyAsync(la => la.LoanApplicationId == model.LoanApplicationId);

                if (!loanApplicationExists)
                {
                    ModelState.AddModelError("LoanApplicationId", "The specified Loan Application does not exist.");
                    return View(model);
                }

                if (model.Receipt != null && model.Receipt.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", model.LoanApplicationId.ToString());

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, model.Receipt.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Receipt.CopyToAsync(stream);
                    }

                    var repayment = new Repayment
                    {
                        ApplicationId = model.LoanApplicationId,
                        AmountPaid = model.AmountPaid,
                        PaymentDate = model.PaymentDate,
                        ReceiptUpload = Path.Combine("uploads", model.LoanApplicationId.ToString(), model.Receipt.FileName)
                    };

                    _context.Repayments.Add(repayment);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Repayment"); // Redirect to the loan applications list
                }
                ModelState.AddModelError("", "Please upload a valid receipt.");
            }

            return View(model);
        }
    }
}
