using System;
using System.Linq;
using hotels.Areas.Admin.Models;
using hotels.Models;
using hotels.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotels.Controllers
{
    public class LoginController : Controller
    {
        private readonly DataContext _context;

        public LoginController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(tblTaiKhoan user)
        {
            if (user == null)
                return NotFound();

            string pw = Functions.MD5Password(user.MatKhau);

            var check = _context.TaiKhoans
                .Include(t => t.KhachHangs)
                .Include(t => t.NhanViens)
                .FirstOrDefault(u => u.TenDangNhap == user.TenDangNhap && u.MatKhau == pw);

            if (check == null)
            {
                Functions._Message = "Tên đăng nhập hoặc mật khẩu không hợp lệ";
                return RedirectToAction("Index", "Login");
            }

            Functions._Message = string.Empty;
            Functions._MaTaiKhoan = check.IDTaiKhoan;
            Functions._TenDangNhap = check.TenDangNhap ?? string.Empty;
            Functions._VaiTro = check.VaiTro;

            if (check.VaiTro == 0)
            {
                Functions._TenNguoiDung = "Quản trị viên";
                Functions._Email = "admin@hotel.vn";
            }
            else if (check.VaiTro == 1)
            {
                var kh = check.KhachHangs?.FirstOrDefault();
                Functions._TenNguoiDung = kh?.HoTen ?? "Khách hàng";
                Functions._Email = kh?.Email ?? string.Empty;
            }
            else if (check.VaiTro == 2)
            {
                var nv = check.NhanViens?.FirstOrDefault();
                Functions._TenNguoiDung = nv?.HoTen ?? "Nhân viên";
                Functions._Email = nv?.Email ?? string.Empty;
            }
            else
            {
                Functions._TenNguoiDung = "Chưa đăng nhập";
                Functions._Email = string.Empty;
            }

            HttpContext.Session.SetInt32("VaiTro", check.VaiTro);
            HttpContext.Session.SetInt32("IDTaiKhoan", (int)check.IDTaiKhoan);

            if (check.VaiTro == 0 || check.VaiTro == 2)
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            else
                return RedirectToAction("Index", "Home");

        }

        public IActionResult Logout()
        {
            Functions._MaTaiKhoan = 0;
            Functions._TenDangNhap = string.Empty;
            Functions._TenNguoiDung = "Chưa đăng nhập";
            Functions._Email = string.Empty;
            Functions._VaiTro = -1;
            Functions._Message = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Login");
        }
    }
}
