using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using hotels.Models;

namespace hotels.Controllers;

public class AuthController : Controller
{

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }
}
