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
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CustomerController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CustomerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<CustomerController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Customer/Profile
        public async Task<IActionResult> Profile()
        {
            _logger.LogInformation("Displaying customer profile.");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Customer user not found.");
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (customer == null)
            {
                var names = user.FullName.Split(' ');
                // Create a new customer profile if it doesn't exist
                customer = new Customer
                {
                    UserId = user.Id,
                    FirstName = names[0],
                    LastName = names.Length > 1 ? names[1] : names[0],
                    Address = string.Empty// Set default.
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Customer profile created.");
            }
            return View(customer);
        }

        public IActionResult UpdateProfile(int id)
        {
            _logger.LogInformation("Editing customer profile with ID {Id}.", id);
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                _logger.LogWarning("Customer not found with ID {Id}.", id);
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("CustomerId,FirstName,LastName,DateOfBirth,Address,UserId")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Customer profile updated successfully.");
                    return RedirectToAction(nameof(Profile));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Error updating customer profile.");
                    ModelState.AddModelError("", "Unable to update your profile. Please try again.");
                    return View(customer);
                }
            }
            _logger.LogWarning("Failed to update customer profile. Model state errors: {ModelStateErrors}", ModelState.Values);
            return View(customer);
        }

        // GET: Customer/ApplyLoan
        public IActionResult ApplyLoan()
        {
            _logger.LogInformation("Displaying Apply Loan form.");
            ViewData["LoanTypeId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.LoanTypes, "LoanTypeId", "LoanTypeName");
            ViewBag.DocumentTypes = new List<string> { "ID Card", "Pay Slip", "Bank Statement", "Other" };
            return View();
        }

        // POST: Customer/ApplyLoan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyLoan([Bind("LoanApplicationId,LoanTypeId,LoanAmount,Purpose")] ApplyForLoanVM model, List<string> documentTypes)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Customer user not found.");
                    return NotFound();
                }

                var loanApplication = new LoanApplication
                {
                    CustomerId = user.Id,
                    LoanTypeId = model.LoanTypeId,
                    AmountRequested = model.LoanAmount,
                    Purpose = model.Purpose,
                    DateApplied = DateTime.UtcNow,
                    Status = (int)LoanApplicationStatus.Pending
                };
                if (Request.Form.Files.Count != documentTypes.Count)
                {
                    ModelState.AddModelError("", "Number of files must match the number of document types.");
                    ViewData["LoanTypeId"] = new SelectList(_context.LoanTypes, "LoanTypeId", "LoanTypeName", model.LoanTypeId);
                    ViewBag.DocumentTypes = new List<string> { "ID Card", "Pay Slip", "Bank Statement", "Other" };
                    return View(model);
                }
                //_context.Add(loanApplication);
                //await _context.SaveChangesAsync();
                //_logger.LogInformation("Loan application submitted successfully.");

                if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                {
                    loanApplication.LoanApplicationDocuments = [];
                    for (int i = 0; i < Request.Form.Files.Count; i++)
                    {
                        var file = Request.Form.Files[i];
                        var documentType = documentTypes[i];

                        if (!file.ContentType.StartsWith("image/") && file.ContentType != "application/pdf")
                        {
                            ModelState.AddModelError("files", $"File type for {file.FileName} must be an image or PDF.");
                            ViewBag.DocumentTypes = new List<string> { "ID Card", "Pay Slip", "Bank Statement", "Other" };
                            return View(model);
                        }

                        if (file != null && file.Length > 0)
                        {
                            if (file.Length > 2 * 1024 * 1024) // 2MB limit
                            {
                                ModelState.AddModelError("files", $"File size for {file.FileName} must be less than 2MB.");
                                ViewData["LoanTypeId"] = new SelectList(_context.LoanTypes, "LoanTypeId", "LoanTypeName", model.LoanTypeId);
                                ViewBag.DocumentTypes = new List<string> { "ID Card", "Pay Slip", "Bank Statement", "Other" };
                                return View(model);
                            }
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string dir = _webHostEnvironment.WebRootPath + "/documents";

                            if (!Directory.Exists(dir))
                                Directory.CreateDirectory(dir);

                            string filePath = Path.Combine(dir, fileName); // Store in wwwroot/documents
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }
                            string docType = "Other"; // Default
                            string docTypeKey = "documentTypes[" + i + "]";  // Construct the expected key
                            if (Request.Form.ContainsKey(docTypeKey))
                            {
                                docType = Request.Form[docTypeKey];
                            }

                            loanApplication.LoanApplicationDocuments.Add(new LoanApplicationDocument
                            {
                                LoanApplicationId = loanApplication.LoanApplicationId, // Use the ID
                                DocumentType = docType,
                                FilePath = "/documents/" + fileName // Store relative path
                            });
                            //loanApplicationDocuments.Add(document);
                        }
                    }
                    _context.LoanApplications.Add(loanApplication);
                    await _context.SaveChangesAsync(); // Save documents
                    _logger.LogInformation("Documents uploaded and loanApplication saved successfully for Loan Application ID {LoanApplicationId}.", loanApplication.LoanApplicationId);
                    return RedirectToAction(nameof(LoanApplications));
                }
            }
            ViewBag.DocumentTypes = new List<string> { "ID Card", "Pay Slip", "Bank Statement", "Other" };
            ViewData["LoanTypeId"] = new SelectList(_context.LoanTypes, "LoanTypeId", "LoanTypeName", model.LoanTypeId);
            _logger.LogWarning("Failed to submit loan application. Model state errors: {ModelStateErrors}", ModelState.Values);
            return View(model);
        }
        // GET: Customer/LoanApplications
        public async Task<IActionResult> LoanApplications()
        {
            _logger.LogInformation("Displaying customer's loan applications.");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Customer user not found.");
                return NotFound();
            }

            var loanApplications = await _context.LoanApplications
                .Include(la => la.LoanType)
                .Include(la => la.Customer)
                .Include(la => la.LoanApplicationDocuments)
                .Where(la => la.CustomerId == user.Id) // Still using User Id
                .ToListAsync();

            var loanApplicationViewModels = loanApplications.Select(la => new LoanApplicationVM
            {
                Id = la.LoanApplicationId,
                LoanTypeName = la.LoanType.LoanTypeName,
                CustomerName = la.Customer.FullName, // Get username
                AmountRequested = la.AmountRequested,
                Purpose = la.Purpose,
                DateApplied = la.DateApplied,
                Status = (LoanApplicationStatus)la.Status,
            }).ToList();
            return View(loanApplicationViewModels);
        }

        // GET: Customer/UploadReceipt/5
        public async Task<IActionResult> UploadReceipt(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Payment ID is null.");
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found with ID {Id}.", id);
                return NotFound();
            }
            ViewBag.PaymentId = id;
            return View();
        }

        // POST: Customer/UploadReceipt/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadReceipt(int id, IFormFile receiptFile)
        {
            if (receiptFile == null || receiptFile.Length == 0)
            {
                ModelState.AddModelError("receiptFile", "Please select a file to upload.");
                _logger.LogWarning("No file uploaded for Payment ID {Id}.", id);
                ViewBag.PaymentId = id;
                return View();
            }

            if (receiptFile.Length > 2 * 1024 * 1024) // 2MB limit
            {
                ModelState.AddModelError("receiptFile", "File size must be less than 2MB.");
                _logger.LogWarning("File too large for Payment ID {Id}.", id);
                ViewBag.PaymentId = id;
                return View();
            }

            // Check file type (basic check, you might want a more robust solution)
            if (!receiptFile.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("receiptFile", "File type must be an image.");
                _logger.LogWarning("Invalid file type for Payment ID {Id}.", id);
                ViewBag.PaymentId = id;
                return View();
            }

            try
            {
                var payment = await _context.Payments.FindAsync(id);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found with ID {Id}.", id);
                    return NotFound();
                }
                // Generate a unique file name
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(receiptFile.FileName);
                var filePath = Path.Combine("wwwroot/receipts", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await receiptFile.CopyToAsync(fileStream);
                }

                payment.ReceiptImageUrl = "/receipts/" + fileName;
                _context.Update(payment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Receipt uploaded successfully for Payment ID {Id}.", id);
                return RedirectToAction(nameof(LoanApplications));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading receipt for Payment ID {Id}.", id);
                ModelState.AddModelError("", "An error occurred while uploading the receipt. Please try again.");
                ViewBag.PaymentId = id;
                return View();
            }
        }
    }
}
