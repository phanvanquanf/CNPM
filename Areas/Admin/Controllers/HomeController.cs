using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using hotels.Models;
using hotels.Filters;
using hotels.Utilities;

namespace hotels.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuthorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Title = "Trang quản lý khách sạn";
        if (!Functions.IsLogin())
            return RedirectToAction("Index", "Login");
        return View();
    }
}
