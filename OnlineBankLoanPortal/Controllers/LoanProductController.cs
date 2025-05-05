using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBankLoanPortal.Repository;

namespace OnlineBankLoanPortal.Controllers
{
    public class LoanProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoanProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LoanProduct
        //public async Task<IActionResult> Index()
        //{
        //    var products = await _context.LoanProducts.ToListAsync();
        //    return View(products);
        //}
    }
}
