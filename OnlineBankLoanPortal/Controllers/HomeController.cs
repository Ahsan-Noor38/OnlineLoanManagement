using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnlineBankLoanPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace OnlineBankLoanPortal.Controllers;

public class HomeController : Controller
{
    private readonly MydbContext context;

    public HomeController(MydbContext context)
    {
        this.context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AboutUS()
    {
        return View();
    }
    public IActionResult Services()
    {
        return View();
    }
    
    public IActionResult ApplyForloan ()
    {
     

        

        return View();
       
    }
    

    
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Login(Customer cst)
    {
        var Mycst = context.Customers.Where(x => x.Email == cst.Email && x.Password == cst.Password).FirstOrDefault();
        if (Mycst!=null)
        {
            HttpContext.Session.SetString("usersession", Mycst.Email);
            return RedirectToAction("Index");
        }
        else
        {
            ViewBag.Message = "Login Failed...";
        }
            return View();
    }
    public IActionResult SignUp()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> SignUp(Customer cst)
    {
        if (ModelState.IsValid)
        {
            await context.Customers.AddAsync(cst);
            await context.SaveChangesAsync();
            TempData["Success"] = "Successfully Registered";
            return RedirectToAction("Login");
        }


    

        return View();
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
