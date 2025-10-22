using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using hotels.Models;

namespace hotels.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
