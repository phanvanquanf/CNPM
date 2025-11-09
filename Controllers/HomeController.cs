using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using hotels.Utilities;
using hotels.Models;
using hotels.Filters;

namespace hotels.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Title = "Trang Chủ - Grandoria Hotel";
        if (!Functions.IsLogin())
            return RedirectToAction("Index", "Login");
        return View();
    }

    public IActionResult Logout()
    {
        Functions._MaTaiKhoan = 0;
        Functions._VaiTro = 0;
        Functions._TenNguoiDung = string.Empty;
        Functions._TenDangNhap = string.Empty;
        Functions._Email = string.Empty;
        Functions._Message = string.Empty;
        return RedirectToAction("Index", "Login");
    }

    public IActionResult About()
    {
        return View();
    }
    public IActionResult Offers()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }
}
