using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineBankLoanPortal.Hubs;
using OnlineBankLoanPortal.Models;
using OnlineBankLoanPortal.Repository;
using OnlineBankLoanPortal.ViewModels;

namespace OnlineBankLoanPortal.Controllers
{
    [Authorize(Roles = "LoanOfficer")]
    public class LoanOfficerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<LoanApplicationHub> _hubContext;
        private readonly ILogger<LoanOfficerController> _logger;

        public LoanOfficerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<LoanApplicationHub> hubContext, ILogger<LoanOfficerController> logger)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            var dashboardData = new AdminDashboardVM();

            // Get counts for different loan application statuses
            var loanApplications = _context.LoanApplications.AsQueryable();
            dashboardData.TotalApplications = await loanApplications.CountAsync();
            dashboardData.PendingApplications = await loanApplications.CountAsync(la => la.Status == (int)LoanApplicationStatus.Pending);
            dashboardData.ApprovedApplications = await loanApplications.CountAsync(la => la.Status == (int)LoanApplicationStatus.Approved);
            dashboardData.RejectedApplications = await loanApplications.CountAsync(la => la.Status == (int)LoanApplicationStatus.Rejected);
            dashboardData.CancelledApplications = await loanApplications.CountAsync(la => la.Status == (int)LoanApplicationStatus.Cancelled);

            // Get recent loan applications (e.g., last 5)
            dashboardData.RecentApplications = await loanApplications
                .Include(la => la.LoanType)
                .Include(la => la.Customer)
                .OrderByDescending(la => la.DateApplied)
                .Take(5)
                .ToListAsync();

            return View(dashboardData);
        }

        // GET: LoanOfficer/LoanApplications
        public async Task<IActionResult> LoanApplications(string sortOrder, string searchString, string statusFilter)
        {
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "Date" : "";
            ViewData["StatusSortParm"] = string.IsNullOrEmpty(sortOrder) ? "Status" : "";
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            var loggedInUser = await _userManager.GetUserAsync(User);
            var loanApplications = await _context.LoanApplications
                .Where(la => la.AssignedLoanOfficerId == loggedInUser.Id)
                //.Where(la => la.Status == (int)LoanApplicationStatus.Pending)
                .Include(la => la.LoanType)
                .Include(la => la.Customer)
                .Include(la => la.LoanApplicationDocuments)
                .ToListAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                loanApplications = loanApplications.Where(s => s.Customer.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!String.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                if (Enum.TryParse(statusFilter, out LoanApplicationStatus status))
                {
                    loanApplications = loanApplications.Where(s => s.Status == (int)status).ToList();
                }
            }

            switch (sortOrder)
            {
                case "Date":
                    loanApplications = loanApplications.OrderBy(s => s.DateApplied).ToList();
                    break;
                case "Status":
                    loanApplications = loanApplications.OrderBy(s => s.Status).ToList();
                    break;
                default:
                    loanApplications = loanApplications.OrderByDescending(s => s.DateApplied).ToList();
                    break;
            }
            var loanApplicationViewModels = loanApplications.Select(la => new LoanApplicationVM
            {
                Id = la.LoanApplicationId,
                LoanTypeName = la.LoanType.LoanTypeName,
                CustomerName = _userManager.FindByIdAsync(la.CustomerId.ToString()).Result?.UserName, // Get username
                AmountRequested = la.AmountRequested,
                Purpose = la.Purpose,
                DateApplied = la.DateApplied,
                Status = (LoanApplicationStatus)la.Status,
            }).ToList();

            return View(loanApplicationViewModels);
        }

        public async Task<IActionResult> ViewApplicationDocuments(int id)
        {
            _logger.LogInformation("Displaying documents for Loan Application ID {Id}.", id);

            var loanApplication = await _context.LoanApplications
                .Where(la => la.LoanApplicationId == id)
                .Include(la => la.Customer)
                .Include(la => la.LoanType)
                .Include(la => la.LoanApplicationDocuments)
                .FirstOrDefaultAsync();

            if (loanApplication == null)
            {
                _logger.LogWarning("Loan Application not found with ID {Id}.", id);
                return NotFound();
            }

            if (loanApplication.LoanApplicationDocuments == null || loanApplication.LoanApplicationDocuments.Count == 0)
            {
                ViewBag.NoDocuments = "No documents have been uploaded for this loan application.";
            }
            return View(loanApplication);
        }

        // GET: LoanOfficer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications
                .Include(la => la.LoanType)
                .Include(la => la.Customer) // Assuming you have a Customer entity
                .FirstOrDefaultAsync(m => m.LoanApplicationId == id);
            if (loanApplication == null)
            {
                return NotFound();
            }

            var loanApplicationDetailsViewModel = new LoanApplicationVM
            {
                Id = loanApplication.LoanApplicationId,
                LoanTypeName = loanApplication.LoanType.LoanTypeName,
                CustomerName = loanApplication.Customer?.UserName,
                AmountRequested = loanApplication.AmountRequested,
                Purpose = loanApplication.Purpose,
                DateApplied = loanApplication.DateApplied,
                Status = (LoanApplicationStatus)loanApplication.Status, // Convert to string
                ApplicationNotes = loanApplication.ApplicationNotes
            };
            return View(loanApplicationDetailsViewModel);
        }

        // GET: LoanOfficer/Approve/5
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications.Where(la => la.LoanApplicationId == id)
                .Include(la => la.LoanType)
                .Include(la => la.Customer)
                .Include(la => la.LoanApplicationDocuments)
                .FirstOrDefaultAsync();
            if (loanApplication == null)
            {
                return NotFound();
            }
            if (loanApplication.Status != (int)LoanApplicationStatus.Pending)
            {
                return RedirectToAction(nameof(LoanApplications));
            }

            var approveViewModel = new LoanApplicationVM
            {
                Id = loanApplication.LoanApplicationId,
                LoanTypeName = loanApplication.LoanType?.LoanTypeName,
                CustomerName = loanApplication.Customer?.FullName,
                AmountRequested = loanApplication.AmountRequested,
                Purpose = loanApplication.Purpose,
                DateApplied = loanApplication.DateApplied,
                Status = (LoanApplicationStatus)loanApplication.Status,
            };
            ViewBag.RepaymentTerms = await _context.RepaymentTerms.ToListAsync();

            return View(approveViewModel);
        }

        // POST: LoanOfficer/Approve/5
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveConfirmed(int id, string applicationNotes, DateTime? expectedDisbursementDate, int? repaymentTermId)
        {
            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                return NotFound();
            }

            if (!expectedDisbursementDate.HasValue || !repaymentTermId.HasValue)
            {
                ModelState.AddModelError("", "Disbursement Date and Repayment Term are required for approval.");
                var approveViewModel = new LoanApplicationVM
                {
                    Id = loanApplication.LoanApplicationId,
                    LoanTypeName = loanApplication.LoanType?.LoanTypeName,
                    CustomerName = loanApplication.Customer?.UserName, //changed to username
                    AmountRequested = loanApplication.AmountRequested,
                    Purpose = loanApplication.Purpose,
                    DateApplied = loanApplication.DateApplied,
                    Status = (LoanApplicationStatus)loanApplication.Status,
                    LoanApplicationDocuments = loanApplication.LoanApplicationDocuments
                };
                ViewBag.RepaymentTerms = await _context.RepaymentTerms.ToListAsync();
                return View("Approve", approveViewModel);
            }

            var repaymentTerm = await _context.RepaymentTerms.FindAsync(repaymentTermId);
            var currentInterestRate = await _context.InterestRates
                .Where(ir => ir.LoanTypeId == loanApplication.LoanTypeId && ir.EffectiveDate <= DateTime.UtcNow && (ir.EndDate == null || ir.EndDate > DateTime.UtcNow))
                .OrderByDescending(ir => ir.EffectiveDate)
                .FirstOrDefaultAsync();

            if (repaymentTerm == null || currentInterestRate == null)
            {
                ModelState.AddModelError("", "Selected repayment term or interest rate is not valid.");
                return View("Approve", loanApplication);
            }

            loanApplication.Status = (int)LoanApplicationStatus.Active; // Update status to Active upon approval
            loanApplication.ApprovedDate = DateTime.UtcNow;
            loanApplication.ApplicationNotes = applicationNotes;
            loanApplication.ExpectedDisbursementDate = expectedDisbursementDate;
            loanApplication.InterestRate = currentInterestRate.Rate;
            loanApplication.RepaymentTermMonths = repaymentTerm.TermInMonths;

            // Basic monthly payment calculation (you'll likely need a more sophisticated one)
            if (loanApplication.InterestRate.HasValue && loanApplication.RepaymentTermMonths.HasValue)
            {
                decimal monthlyInterestRate = loanApplication.InterestRate.Value / 1200; // Monthly rate
                loanApplication.MonthlyPayment = CalculateMonthlyPayment(loanApplication.AmountRequested, monthlyInterestRate, loanApplication.RepaymentTermMonths.Value);
                loanApplication.OutstandingBalance = loanApplication.AmountRequested; // Initial balance
            }

            var user = await _userManager.GetUserAsync(User);
            loanApplication.ApprovedBy = user?.Id != null ? user.Id : (Guid?)null;
            _context.Update(loanApplication);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                "LoanApplicationStatusChanged",
                loanApplication.LoanApplicationId,
                Enum.GetName(typeof(LoanApplicationStatus), (LoanApplicationStatus)loanApplication.Status));

            return RedirectToAction(nameof(LoanApplications));
        }

        private decimal CalculateMonthlyPayment(decimal principal, decimal monthlyInterestRate, int termInMonths)
        {
            if (monthlyInterestRate == 0)
            {
                return principal / termInMonths;
            }
            return (principal * monthlyInterestRate * (decimal)Math.Pow(1 + (double)monthlyInterestRate, termInMonths)) /
                   ((decimal)Math.Pow(1 + (double)monthlyInterestRate, termInMonths) - 1);
        }

        // GET: LoanOfficer/Reject/5
        public async Task<IActionResult> Reject(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                return NotFound();
            }
            if (loanApplication.Status != (int)LoanApplicationStatus.Pending)
            {
                return RedirectToAction(nameof(LoanApplications)); //or show an error.
            }

            var rejectViewModel = new LoanApplicationVM
            {
                Id = loanApplication.LoanApplicationId,
                LoanTypeName = loanApplication.LoanType?.LoanTypeName,
                CustomerName = loanApplication.Customer?.UserName,
                AmountRequested = loanApplication.AmountRequested,
                Purpose = loanApplication.Purpose,
                DateApplied = loanApplication.DateApplied,
                Status = (LoanApplicationStatus)loanApplication.Status, // Convert to string
            };

            return View(rejectViewModel);
        }

        // POST: LoanOfficer/Reject/5
        [HttpPost, ActionName("Reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectConfirmed(int id, LoanApplicationVM model)
        {
            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                return NotFound();
            }

            loanApplication.Status = (int)LoanApplicationStatus.Rejected;
            loanApplication.ApprovedDate = DateTime.UtcNow;
            loanApplication.ApplicationNotes = model.ApplicationNotes;

            // Get the current user (Loan Officer)
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                loanApplication.ApprovedBy = user.Id; // Store Loan Officer's ID
            }
            else
            {
                loanApplication.ApprovedBy = null;
            }

            _context.Update(loanApplication);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                "LoanApplicationStatusChanged",
                loanApplication.LoanApplicationId,
                Enum.GetName(typeof(LoanApplicationStatus), (LoanApplicationStatus)loanApplication.Status));

            return RedirectToAction(nameof(LoanApplications));
        }

        // GET: LoanOfficer/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                return NotFound();
            }
            if (loanApplication.Status != (int)LoanApplicationStatus.Pending)
            {
                return RedirectToAction(nameof(LoanApplications)); // Or show error
            }

            var CancelViewModel = new LoanApplicationVM
            {
                Id = loanApplication.LoanApplicationId,
                LoanTypeName = loanApplication.LoanType?.LoanTypeName,
                CustomerName = loanApplication.Customer?.UserName,
                AmountRequested = loanApplication.AmountRequested,
                Purpose = loanApplication.Purpose,
                DateApplied = loanApplication.DateApplied,
                Status = (LoanApplicationStatus)loanApplication.Status, // Convert to string
            };

            return View(CancelViewModel);

            return View(loanApplication);
        }

        // POST: LoanOfficer/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, LoanApplicationVM model)
        {
            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                return NotFound();
            }

            loanApplication.Status = (int)LoanApplicationStatus.Cancelled;
            loanApplication.CancelledDate = DateTime.UtcNow;
            loanApplication.CancellationReason = model.CancellationReason;

            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                loanApplication.CancelledBy = user.Id.ToString();
            }
            else
            {
                loanApplication.CancelledBy = "Unknown";
            }

            _context.Update(loanApplication);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                "LoanApplicationStatusChanged",
                loanApplication.LoanApplicationId,
                Enum.GetName(typeof(LoanApplicationStatus), (LoanApplicationStatus)loanApplication.Status));

            return RedirectToAction(nameof(LoanApplications));
        }

        // GET: LoanOfficer/TrackRepayments/5
        public async Task<IActionResult> TrackRepayments(int? id)
        {
            _logger.LogInformation("Displaying repayment tracking for loan application ID: {Id}", id);
            if (id == null)
            {
                _logger.LogWarning("Loan application ID is null.");
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan application not found with ID: {Id}", id);
                return NotFound();
            }
            return View(loanApplication);
        }

        // POST: LoanOfficer/ProcessRepayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessRepayment(int id, decimal paymentAmount)
        {
            _logger.LogInformation("Processing repayment of {PaymentAmount} for loan application ID: {Id}", paymentAmount, id);
            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan application not found with ID: {Id}", id);
                return NotFound();
            }

            if (loanApplication.Status != (int)LoanApplicationStatus.Active && loanApplication.Status != (int)LoanApplicationStatus.Overdue)
            {
                _logger.LogWarning("Loan application ID: {Id} is not active or overdue. Status: {Status}", id, loanApplication.Status);
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
                    loanApplication.OutstandingBalance -= paymentAmount;
                    loanApplication.LastRepaymentDate = DateTime.UtcNow;

                    if (loanApplication.OutstandingBalance <= 0)
                    {
                        loanApplication.Status = (int)LoanApplicationStatus.Closed;
                        _logger.LogInformation("Loan application ID: {Id} marked as Closed.", id);
                    }
                    else if (loanApplication.OutstandingBalance > 0 && DateTime.UtcNow > loanApplication.ExpectedDisbursementDate?.AddMonths(loanApplication.RepaymentTermMonths ?? 0)) // Basic overdue check
                    {
                        loanApplication.Status = (int)LoanApplicationStatus.Overdue;
                        _logger.LogWarning("Loan application ID: {Id} marked as Overdue.", id);
                    }
                    else if (loanApplication.OutstandingBalance > 0)
                    {
                        loanApplication.Status = (int)LoanApplicationStatus.Active; // Ensure it stays active if still balance
                    }

                    _context.Update(loanApplication);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Repayment of {PaymentAmount} processed for loan application ID: {Id}. New balance: {Balance}, Status: {Status}", paymentAmount, id, loanApplication.OutstandingBalance, loanApplication.Status);

                    await _hubContext.Clients.All.SendAsync(
                        "LoanApplicationStatusChanged",
                        loanApplication.LoanApplicationId,
                        loanApplication.Status.ToString());

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
                _logger.LogError(ex, "Error occurred while processing repayment for loan application ID: {Id}", id);
                return View("Error");
            }
        }

        // GET: LoanOfficer/Communicate/5
        public async Task<IActionResult> Communicate(string id) // Using customer ID
        {
            _logger.LogInformation("Displaying communication page for customer ID: {CustomerId}", id);
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Customer ID is null or empty.");
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Customer not found with ID: {CustomerId}", id);
                return NotFound();
            }
            ViewBag.CustomerId = id;
            ViewBag.CustomerName = user.UserName;
            return View();
        }

        // POST: LoanOfficer/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string customerId, string message)
        {
            _logger.LogInformation("Sending message to customer ID: {CustomerId}. Message: {Message}", customerId, message);
            if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("Customer ID or message is null or empty.");
                ModelState.AddModelError("", "Customer ID and message are required.");
                return View("Communicate", new { id = customerId });
            }

            // In a real application, you would likely:
            // 1. Store the message in a database (e.g., a CommunicationLog table).
            // 2. Implement a mechanism to notify the customer (e.g., email, in-app notification).

            _logger.LogInformation("Message sent to customer ID: {CustomerId}. Message: {Message}", customerId, message);
            TempData["MessageSent"] = "Message sent successfully."; // For displaying a confirmation

            return RedirectToAction(nameof(Communicate), new { id = customerId });
        }

        // GET: LoanOfficer/UpdateStatus/5
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            _logger.LogInformation("Displaying update status page for loan application ID: {Id}", id);
            if (id == null)
            {
                _logger.LogWarning("Loan application ID is null.");
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan application not found with ID: {Id}", id);
                return NotFound();
            }
            return View(loanApplication);
        }

        // POST: LoanOfficer/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatusConfirmed(int id, LoanApplicationStatus newStatus)
        {
            _logger.LogInformation("Updating status of loan application ID: {Id} to {NewStatus}", id, newStatus);
            var loanApplication = await _context.LoanApplications.FindAsync(id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan application not found with ID: {Id}", id);
                return NotFound();
            }

            if (loanApplication.Status == (int)newStatus)
            {
                _logger.LogInformation("Loan application ID: {Id} is already in status {NewStatus}.", id, newStatus);
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                loanApplication.Status = (int)newStatus;
                _context.Update(loanApplication);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Status of loan application ID: {Id} updated to {NewStatus}.", id, newStatus);

                await _hubContext.Clients.All.SendAsync(
                    "LoanApplicationStatusChanged",
                    loanApplication.LoanApplicationId,
                    loanApplication.Status.ToString());

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating status of loan application ID: {Id} to {NewStatus}.", id, newStatus);
                return View("Error");
            }
        }

        // GET: LoanOfficer/ReviewApplication/5
        public async Task<IActionResult> ReviewApplication(int? id)
        {
            _logger.LogInformation("Displaying review page for loan application ID: {Id}", id);
            if (id == null)
            {
                _logger.LogWarning("Loan application ID is null.");
                return NotFound();
            }

            var loanApplication = await _context.LoanApplications
                .Include(la => la.LoanType)
                //.Include(la => la.Customer)  // Remove direct customer include
                .FirstOrDefaultAsync(m => m.LoanApplicationId == id);
            if (loanApplication == null)
            {
                _logger.LogWarning("Loan application not found with ID: {Id}", id);
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(loanApplication.CustomerId.ToString());
            if (user == null)
            {
                _logger.LogWarning("Customer not found with ID: {CustomerId}", loanApplication.CustomerId);
                return NotFound(); // Or handle the error as appropriate
            }
            ViewBag.CustomerName = user.UserName; // Pass the customer name

            // Pass customer ID to the view for the communication link
            ViewBag.CustomerIdForCommunication = loanApplication.CustomerId;

            return View(loanApplication);
        }

    }
}
