using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using hotels.Areas.Admin.Models;
using hotels.Models;
using hotels.Utilities;

namespace hotels.Controllers
{
    public class RegisterController : Controller
    {
        private readonly DataContext _context;

        public RegisterController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(tblTaiKhoan taiKhoan, string hoTen, string gioiTinh, string cccd, string sdt, string email, string diaChi)
        {
            if (taiKhoan == null) return NotFound();

            ViewBag.TaiKhoan = taiKhoan;
            ViewBag.HoTen = hoTen;
            ViewBag.GioiTinh = gioiTinh;
            ViewBag.CCCD = cccd;
            ViewBag.SDT = sdt;
            ViewBag.Email = email;
            ViewBag.DiaChi = diaChi;

            if (string.IsNullOrWhiteSpace(taiKhoan.TenDangNhap)) ModelState.AddModelError("TenDangNhap", "Vui lòng nhập tên đăng nhập!");
            if (string.IsNullOrWhiteSpace(taiKhoan.MatKhau)) ModelState.AddModelError("MatKhau", "Vui lòng nhập mật khẩu!");

            if (_context.TaiKhoans.Any(u => u.TenDangNhap == taiKhoan.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                taiKhoan.TenDangNhap = "";
            }
            if (_context.KhachHangs.Any(u => u.SDT == sdt) || _context.NhanViens.Any(u => u.SDT == sdt))
            {
                ModelState.AddModelError("SDT", "Số điện thoại đã tồn tại");
                sdt = "";
            }
            if (_context.KhachHangs.Any(u => u.Email == email) || _context.NhanViens.Any(u => u.Email == email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                email = "";
            }
            if (_context.KhachHangs.Any(u => u.CCCD == cccd) || _context.NhanViens.Any(u => u.CCCD == cccd))
            {
                ModelState.AddModelError("CCCD", "CCCD đã tồn tại");
                cccd = "";
            }

            if (!ModelState.IsValid) return View(taiKhoan);

            taiKhoan.TrangThai = true;
            taiKhoan.MatKhau = Functions.MD5Password(taiKhoan.MatKhau);
            taiKhoan.NgayTao = DateTime.Now;
            taiKhoan.VaiTro = 1;

            _context.TaiKhoans.Add(taiKhoan);
            _context.SaveChanges();

            _context.KhachHangs.Add(new tblKhachHang
            {
                IDTaiKhoan = taiKhoan.IDTaiKhoan,
                HoTen = hoTen,
                GioiTinh = gioiTinh,
                CCCD = cccd,
                SDT = sdt,
                Email = email,
                DiaChi = diaChi,
                TrangThai = 0
            });

            _context.SaveChanges();

            return RedirectToAction("Index", "Login");
        }
    }
}
