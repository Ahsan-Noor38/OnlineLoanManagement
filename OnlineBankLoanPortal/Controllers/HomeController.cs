using Microsoft.AspNetCore.Mvc;
using OnlineBankLoanPortal.Models;
using System.Diagnostics;

namespace OnlineBankLoanPortal.Controllers;

public class HomeController : Controller
{

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

    //public IActionResult ApplyForloan()
    //{

    //    return View();

    //}


    //[HttpPost]
    //public IActionResult Login(Customer cst)
    //{
    //    var Mycst = context.Customers.Where(x => x.Email == cst.Email && x.Password == cst.Password).FirstOrDefault();
    //    if (Mycst != null)
    //    {
    //        HttpContext.Session.SetString("usersession", Mycst.Email);
    //        return RedirectToAction("Index");
    //    }
    //    else
    //    {
    //        ViewBag.Message = "Login Failed...";
    //    }
    //    return View();
    //}

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
