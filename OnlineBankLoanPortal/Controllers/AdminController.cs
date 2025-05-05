using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineBankLoanPortal.Models;
using OnlineBankLoanPortal.Repository;
using OnlineBankLoanPortal.ViewModels;

namespace OnlineBankLoanPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // 4. Loan Application Oversight and Assignment
        // GET: Admin/LoanApplications
        public async Task<IActionResult> LoanApplications()
        {
            _logger.LogInformation("Fetching all loan applications.");
            var loanApplications = await _context.LoanApplications
                .Include(la => la.LoanType)
                .Include(la => la.LoanOfficer) // Include LoanOfficer
                .Include(la => la.Customer)
                .Select(la => new LoanApplicationVM
                {
                    Id = la.LoanApplicationId,
                    LoanTypeName = la.LoanType.LoanTypeName,
                    CustomerName = la.Customer.FullName,
                    OfficerName = la.LoanOfficer.FullName,
                    ActualDisbursementDate = la.ActualDisbursementDate,
                    AmountRequested = la.AmountRequested,
                    DateApplied = la.DateApplied,
                    Purpose = la.Purpose,
                    AssignedOfficerId = la.AssignedLoanOfficerId,
                    Status = (LoanApplicationStatus)la.Status,
                })
                .ToListAsync();

            return View(loanApplications);
        }

        // GET: Admin/LoanApplications/Details/5
        public async Task<IActionResult> DetailsLoanApplication(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Loan Application ID is null.");
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications
                .Include(la => la.LoanType)
                .Include(la => la.LoanOfficer) // Include LoanOfficer
                .Include(la => la.Customer) // Include Customer
                .FirstOrDefaultAsync(m => m.LoanApplicationId == id);
            //loanApplication. = user?.UserName;
            _logger.LogInformation("Fetching details for loan application ID: {Id}", id);
            return View(loanApplication);
        }

        // GET: Admin/LoanApplications/Assign/5
        public async Task<IActionResult> AssignLoanOfficer(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Loan Application ID is null.");
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan Application not found with ID: {Id}", id);
                return NotFound();
            }

            // Get all loan officers
            var loanOfficers = await _userManager.GetUsersInRoleAsync("LoanOfficer");
            ViewBag.LoanOfficers = new SelectList(loanOfficers, "Id", "FullName", loanApplication.AssignedLoanOfficerId);
            _logger.LogInformation("Displaying assign loan officer form for loan application ID: {Id}", id);
            return View(loanApplication);
        }

        // POST: Admin/AssignLoanOfficer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignLoanOfficer(int id, string loanOfficerId)
        {
            _logger.LogInformation("Attempting to assign Loan Officer {LoanOfficerId} to Loan Application ID {Id}.", loanOfficerId, id);
            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan Application not found with ID {Id}.", id);
                return NotFound();
            }

            var loanOfficer = await _userManager.FindByIdAsync(loanOfficerId);
            if (loanOfficer == null)
            {
                _logger.LogWarning("Loan Officer not found with ID {LoanOfficerId}.", loanOfficerId);
                ModelState.AddModelError("", "Loan Officer not found.");
                var loanOfficers = await _userManager.GetUsersInRoleAsync("LoanOfficer");
                ViewBag.LoanOfficers = loanOfficers.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName
                });
                return View(loanApplication);
            }

            try
            {
                loanApplication.AssignedLoanOfficerId = Guid.Parse(loanOfficerId);
                _context.Update(loanApplication);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Loan Officer {LoanOfficerId} assigned to Loan Application ID {Id} successfully.", loanOfficerId, id);
                return RedirectToAction(nameof(LoanApplications));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while assigning Loan Officer to Loan Application ID {Id}.", id);
                ModelState.AddModelError("", "An error occurred while assigning the loan officer.");
                var loanOfficers = await _userManager.GetUsersInRoleAsync("LoanOfficer");
                ViewBag.LoanOfficers = loanOfficers.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName
                });
                return View(loanApplication);
            }
        }

        // GET: Admin/TrackDisbursement/5
        public async Task<IActionResult> TrackDisbursement(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Loan Application ID is null.");
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan Application not found with ID {Id}.", id);
                return NotFound();
            }
            _logger.LogInformation("Displaying Track Disbursement form for Loan Application ID {Id}.", id);
            return View(loanApplication);
        }

        // POST: Admin/TrackDisbursement/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrackDisbursement(int id, DateTime actualDisbursementDate)
        {
            _logger.LogInformation("Attempting to track disbursement for Loan Application ID {Id}.", id);
            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan Application not found with ID {Id}.", id);
                return NotFound();
            }

            if (actualDisbursementDate == default(DateTime))
            {
                ModelState.AddModelError("actualDisbursementDate", "Please enter a valid disbursement date.");
                return View(loanApplication);
            }
            try
            {
                loanApplication.ActualDisbursementDate = actualDisbursementDate;
                loanApplication.Status = (int)LoanApplicationStatus.Active; //update status
                _context.Update(loanApplication);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Disbursement tracked for Loan Application ID {Id} with date {DisbursementDate}.", id, actualDisbursementDate);
                return RedirectToAction(nameof(LoanApplications));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while tracking disbursement for Loan Application ID {Id}.", id);
                ModelState.AddModelError("", "An error occurred while tracking disbursement.");
                return View(loanApplication);
            }
        }

        // GET: Admin/TrackRepayments/5
        public async Task<IActionResult> TrackRepayments(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Loan Application ID is null.");
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications
                .Include(la => la.LoanType)
                .Include(la => la.Customer) // Include Customer for name
                .FirstOrDefaultAsync(la => la.LoanApplicationId == id);

            if (loanApplication == null)
            {
                _logger.LogWarning("Loan Application not found with ID {Id}.", id);
                return NotFound();
            }
            //var user = await _userManager.FindByIdAsync(loanApplication.CustomerId);
            //loanApplication.CustomerName = user?.UserName;
            _logger.LogInformation("Displaying Track Repayments for Loan Application ID {Id}.", id);
            return View(loanApplication);
        }

        // POST: Admin/ProcessRepayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessRepayment(int id, decimal paymentAmount)
        {
            _logger.LogInformation("Processing repayment of {PaymentAmount} for Loan Application ID {Id}.", paymentAmount, id);
            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan Application not found with ID {Id}.", id);
                return NotFound();
            }

            if (loanApplication.Status != (int)LoanApplicationStatus.Active && loanApplication.Status != (int)LoanApplicationStatus.Overdue)
            {
                _logger.LogWarning("Loan Application ID: {Id} is not active or overdue. Status: {Status}", id, loanApplication.Status);
                ModelState.AddModelError("", "Cannot process repayment for this loan status.");
                return View("TrackRepayments", loanApplication);
            }

            if (paymentAmount <= 0)
            {
                ModelState.AddModelError("", "Payment amount must be greater than zero.");
                return View("TrackRepayments", loanApplication);
            }

            try
            {
                if (loanApplication.OutstandingBalance.HasValue)
                {
                    // 1. Create a new Payment record
                    var payment = new Payment
                    {
                        LoanApplicationId = id,
                        Amount = paymentAmount,
                        PaymentDate = DateTime.UtcNow
                    };
                    _context.Payments.Add(payment);

                    // 2. Update the LoanApplication
                    loanApplication.OutstandingBalance -= paymentAmount;
                    loanApplication.LastRepaymentDate = DateTime.UtcNow;

                    if (loanApplication.OutstandingBalance <= 0)
                    {
                        loanApplication.Status = (int)LoanApplicationStatus.Closed;
                        _logger.LogInformation("Loan Application ID: {Id} marked as Closed.", id);
                    }
                    else if (loanApplication.OutstandingBalance > 0 && DateTime.UtcNow > loanApplication.ExpectedDisbursementDate?.AddMonths(loanApplication.RepaymentTermMonths ?? 0))
                    {
                        loanApplication.Status = (int)LoanApplicationStatus.Overdue;
                        _logger.LogWarning("Loan Application ID: {Id} marked as Overdue.", id);
                    }
                    else
                    {
                        loanApplication.Status = (int)LoanApplicationStatus.Active;
                    }

                    _context.Update(loanApplication);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation(
                        "Repayment of {PaymentAmount} processed for Loan Application ID: {Id}. New balance: {Balance}, Status: {Status}",
                        paymentAmount, id, loanApplication.OutstandingBalance, loanApplication.Status);

                    return RedirectToAction(nameof(TrackRepayments), new { id = loanApplication.LoanApplicationId });
                }
                else
                {
                    _logger.LogError("Outstanding balance is null for loan application ID: {Id}", id);
                    ModelState.AddModelError("", "Error processing repayment. Outstanding balance is missing.");
                    return View("TrackRepayments", loanApplication);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing repayment for Loan Application ID: {Id}", id);
                return View("Error");
            }
        }

        #region Loan Type Management

        // GET: Admin/LoanTypes
        public async Task<IActionResult> LoanTypes()
        {
            var loanTypes = await _context.LoanTypes.ToListAsync();
            return View(loanTypes);
        }

        // GET: Admin/LoanTypes/Details/5
        public async Task<IActionResult> DetailsLoanType(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Loan Type ID is null.");
                return NotFound();
            }

            var loanType = await _context.LoanTypes.FirstOrDefaultAsync(m => m.LoanTypeId == id);
            if (loanType == null)
            {
                _logger.LogWarning("Loan Type not found with ID: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("Fetching details for loan type ID: {Id}", id);
            return View(loanType);
        }

        // GET: Admin/CreateLoanType
        public IActionResult CreateLoanType()
        {
            return View();
        }

        // POST: Admin/LoanTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLoanType([Bind("LoanTypeId,LoanTypeName,Description")] LoanType loanType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loanType);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Loan type created with ID: {Id}", loanType.LoanTypeId);
                return RedirectToAction(nameof(LoanTypes));
            }
            _logger.LogWarning("Invalid model state in CreateLoanType.");
            return View(loanType);
        }

        // GET: Admin/EditLoanType/5
        public async Task<IActionResult> EditLoanType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loanType = await _context.LoanTypes.FindAsync(id);
            if (loanType == null)
            {
                return NotFound();
            }
            return View(loanType);
        }

        // POST: Admin/EditLoanType/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLoanType(int id, LoanType loanType)
        {
            if (id != loanType.LoanTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    loanType.UpdatedDate = DateTime.UtcNow;
                    _context.Update(loanType);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(LoanTypes));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoanTypeExists(loanType.LoanTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(loanType);
        }

        // GET: Admin/DeleteLoanType/5
        public async Task<IActionResult> DeleteLoanType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loanType = await _context.LoanTypes.FirstOrDefaultAsync(m => m.LoanTypeId == id);
            if (loanType == null)
            {
                return NotFound();
            }

            return View(loanType);
        }

        // POST: Admin/DeleteLoanType/5
        [HttpPost, ActionName("DeleteLoanType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLoanTypeConfirmed(int id)
        {
            var loanType = await _context.LoanTypes.FindAsync(id);
            if (loanType != null)
            {
                _context.LoanTypes.Remove(loanType);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(LoanTypes));
        }

        private bool LoanTypeExists(int id)
        {
            return _context.LoanTypes.Any(e => e.LoanTypeId == id);
        }

        #endregion

        #region Interest Rate Management

        // GET: Admin/InterestRates
        public async Task<IActionResult> InterestRates()
        {
            var interestRates = await _context.InterestRates.Include(ir => ir.LoanType).ToListAsync();
            return View(interestRates);
        }

        // GET: Admin/InterestRates/Details/5
        public async Task<IActionResult> DetailsInterestRate(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Interest Rate ID is null.");
                return NotFound();
            }

            var interestRate = await _context.InterestRates
                .Include(i => i.LoanType)
                .FirstOrDefaultAsync(m => m.InterestRateId == id);
            if (interestRate == null)
            {
                _logger.LogWarning("Interest Rate not found with ID: {Id}", id);
                return NotFound();
            }
            _logger.LogInformation("Fetching details for interest rate ID: {Id}", id);
            return View(interestRate);
        }

        // GET: Admin/CreateInterestRate
        public async Task<IActionResult> CreateInterestRate()
        {
            ViewBag.LoanTypeId = new SelectList(await _context.LoanTypes.ToListAsync(), "LoanTypeId", "LoanTypeName");
            return View();
        }

        // POST: Admin/InterestRates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInterestRate([Bind("InterestRateId,LoanTypeId,Rate,EffectiveDate,EndDate")] InterestRate interestRate)
        {
            if (ModelState.IsValid)
            {
                _context.Add(interestRate);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Interest rate created with ID: {Id}", interestRate.InterestRateId);
                return RedirectToAction(nameof(InterestRates));
            }
            ViewData["LoanTypeId"] = new SelectList(await _context.LoanTypes.ToListAsync(), "LoanTypeId", "LoanTypeName", interestRate.LoanTypeId);
            _logger.LogWarning("Invalid model state in CreateInterestRate.");
            return View(interestRate);
        }

        // GET: Admin/EditInterestRate/5
        public async Task<IActionResult> EditInterestRate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var interestRate = await _context.InterestRates.FindAsync(id);
            if (interestRate == null)
            {
                return NotFound();
            }
            ViewBag.LoanTypeId = new SelectList(await _context.LoanTypes.ToListAsync(), "LoanTypeId", "LoanTypeName", interestRate.LoanTypeId);
            return View(interestRate);
        }

        // POST: Admin/EditInterestRate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInterestRate(int id, InterestRate interestRate)
        {
            if (id != interestRate.InterestRateId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    interestRate.UpdatedDate = DateTime.UtcNow;
                    _context.Update(interestRate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(InterestRates));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InterestRateExists(interestRate.InterestRateId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.LoanTypeId = new SelectList(await _context.LoanTypes.ToListAsync(), "LoanTypeId", "LoanTypeName", interestRate.LoanTypeId);
            return View(interestRate);
        }

        // GET: Admin/DeleteInterestRate/5
        public async Task<IActionResult> DeleteInterestRate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var interestRate = await _context.InterestRates.Include(ir => ir.LoanType).FirstOrDefaultAsync(m => m.InterestRateId == id);
            if (interestRate == null)
            {
                return NotFound();
            }

            return View(interestRate);
        }

        // POST: Admin/InterestRates/Delete/5
        [HttpPost, ActionName("DeleteInterestRate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedInterestRate(int id)
        {
            var interestRate = await _context.InterestRates.FindAsync(id);
            if (interestRate == null)
            {
                _logger.LogWarning("Interest Rate not found with ID: {Id}", id);
                return NotFound();
            }
            _context.InterestRates.Remove(interestRate);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Interest rate deleted with ID: {Id}", id);
            return RedirectToAction(nameof(InterestRates));
        }

        private bool InterestRateExists(int id)
        {
            return _context.InterestRates.Any(e => e.InterestRateId == id);
        }

        #endregion

        #region Repayment Term Management

        // GET: Admin/RepaymentTerms
        public async Task<IActionResult> RepaymentTerms()
        {
            var repaymentTerms = await _context.RepaymentTerms.Include(rt => rt.LoanType).ToListAsync();
            return View(repaymentTerms);
        }

        // GET: Admin/RepaymentTerms/Details/5
        public async Task<IActionResult> DetailsRepaymentTerm(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Repayment Term ID is null.");
                return NotFound();
            }

            var repaymentTerm = await _context.RepaymentTerms.FirstOrDefaultAsync(m => m.RepaymentTermId == id);
            if (repaymentTerm == null)
            {
                _logger.LogWarning("Repayment Term not found with ID: {Id}", id);
                return NotFound();
            }
            _logger.LogInformation("Fetching details for repayment term ID: {Id}", id);
            return View(repaymentTerm);
        }

        // GET: Admin/CreateRepaymentTerm
        public async Task<IActionResult> CreateRepaymentTerm()
        {
            ViewBag.LoanTypeId = new SelectList(await _context.LoanTypes.ToListAsync(), "LoanTypeId", "LoanTypeName");
            return View();
        }

        // POST: Admin/CreateRepaymentTerm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRepaymentTerm(RepaymentTerm repaymentTerm)
        {
            if (ModelState.IsValid)
            {
                repaymentTerm.CreatedDate = DateTime.UtcNow;
                _context.Add(repaymentTerm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(RepaymentTerms));
            }
            ViewBag.LoanTypeId = new SelectList(await _context.LoanTypes.ToListAsync(), "LoanTypeId", "LoanTypeName", repaymentTerm.LoanTypeId);
            return View(repaymentTerm);
        }

        // GET: Admin/EditRepaymentTerm/5
        public async Task<IActionResult> EditRepaymentTerm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var repaymentTerm = await _context.RepaymentTerms.FindAsync(id);
            if (repaymentTerm == null)
            {
                return NotFound();
            }
            ViewBag.LoanTypeId = new SelectList(await _context.LoanTypes.ToListAsync(), "LoanTypeId", "LoanTypeName", repaymentTerm.LoanTypeId);
            return View(repaymentTerm);
        }

        // POST: Admin/EditRepaymentTerm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRepaymentTerm(int id, RepaymentTerm repaymentTerm)
        {
            if (id != repaymentTerm.RepaymentTermId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    repaymentTerm.UpdatedDate = DateTime.UtcNow;
                    _context.Update(repaymentTerm);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(RepaymentTerms));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RepaymentTermExists(repaymentTerm.RepaymentTermId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(RepaymentTerms));
            }
            ViewBag.LoanTypeId = new SelectList(await _context.LoanTypes.ToListAsync(), "LoanTypeId", "LoanTypeName", repaymentTerm.LoanTypeId);
            return View(repaymentTerm);
        }

        // GET: Admin/DeleteRepaymentTerm/5
        public async Task<IActionResult> DeleteRepaymentTerm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var repaymentTerm = await _context.RepaymentTerms.FirstOrDefaultAsync(m => m.RepaymentTermId == id);
            if (repaymentTerm == null)
            {
                return NotFound();
            }

            return View(repaymentTerm);
        }

        // POST: Admin/DeleteRepaymentTerm/5
        [HttpPost, ActionName("DeleteRepaymentTerm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRepaymentTermConfirmed(int id)
        {
            var repaymentTerm = await _context.RepaymentTerms.FindAsync(id);
            if (repaymentTerm == null)
            {
                _logger.LogWarning("Repayment Term not found with ID: {Id}", id);
                return NotFound();
            }
            _context.RepaymentTerms.Remove(repaymentTerm);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Repayment term deleted with ID: {Id}", id);
            return RedirectToAction(nameof(RepaymentTerms));
        }

        private bool RepaymentTermExists(int id)
        {
            return _context.RepaymentTerms.Any(e => e.RepaymentTermId == id);
        }

        #endregion
    }
}
