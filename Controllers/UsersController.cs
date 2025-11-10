using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hotels.Models;
using hotels.Utilities;
using hotels.Areas.Admin.Models;
using System.Text.RegularExpressions;

namespace hotels.Controllers
{
    public class UsersController : Controller
    {
        private readonly DataContext _context;

        public UsersController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Profile()
        {
            var idTaiKhoan = HttpContext.Session.GetInt32("IDTaiKhoan");
            if (idTaiKhoan == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var khachHang = _context.KhachHangs
                .FirstOrDefault(kh => kh.IDTaiKhoan == idTaiKhoan);

            if (khachHang == null)
            {
                return RedirectToAction("Index", "Login");
            }

            long userId = khachHang.IDKhachHang;

            var datPhongs = _context.DatPhongs
                .Include(dp => dp.CTDatPhongs!)
                    .ThenInclude(ctdp => ctdp.Phong!)
                        .ThenInclude(p => p.LoaiPhong!)
                .Include(dp => dp.CTDatPhongs!)
                    .ThenInclude(ctdp => ctdp.Phong!)
                        .ThenInclude(p => p.AnhPhongs!)
                .Where(dp => dp.IDKhachHang == userId)
                .OrderByDescending(dp => dp.NgayTao)
                .ToList();

            var dpGanNhat = datPhongs.FirstOrDefault();
            ViewBag.dpGanNhat = dpGanNhat;

            var lichSuDichVu = _context.DatDVs
                .Include(d => d.DichVu!)
                    .ThenInclude(dv => dv.LoaiDichVu!)
                .Where(d => d.IDTaiKhoan == idTaiKhoan.Value)
                .OrderByDescending(d => d.NgaySuDung)
                .ToList();

            ViewBag.LichSuDichVu = lichSuDichVu;

            return View(datPhongs);
        }


        [HttpGet]
        public IActionResult EditProfile()
        {
            var idTaiKhoan = HttpContext.Session.GetInt32("IDTaiKhoan");
            if (idTaiKhoan == null)
                return RedirectToAction("Index", "Login");

            var khachHang = _context.KhachHangs
                               .Include(k => k.TaiKhoan)
                               .FirstOrDefault(k => k.IDTaiKhoan == idTaiKhoan);

            if (khachHang == null)
                return NotFound("Không tìm thấy thông tin khách hàng.");

            return View(khachHang);
        }
        [HttpPost]
        public IActionResult EditProfile(tblKhachHang model, string MatKhauCu, string MatKhauMoi, string XacNhanMatKhauMoi)
        {
            var idTaiKhoan = HttpContext.Session.GetInt32("IDTaiKhoan");
            if (idTaiKhoan == null)
                return RedirectToAction("Index", "Login");

            var taiKhoan = _context.TaiKhoans.FirstOrDefault(t => t.IDTaiKhoan == idTaiKhoan);
            var khachHang = _context.KhachHangs
                                .Include(k => k.TaiKhoan)
                                .FirstOrDefault(k => k.IDKhachHang == model.IDKhachHang);

            if (taiKhoan == null || khachHang == null)
                return NotFound();

            var oldSDT = khachHang.SDT;
            var oldCCCD = khachHang.CCCD;
            var oldEmail = khachHang.Email;

            bool hasError = false;

            if (string.IsNullOrWhiteSpace(model.HoTen))
            {
                ModelState.AddModelError("HoTen", "Họ tên không được để trống");
                hasError = true;
            }

            if (!Regex.IsMatch(model.SDT ?? "", @"^\d{10}$"))
            {
                ModelState.AddModelError("SDT", "Số điện thoại phải đúng 10 số");
                hasError = true;
            }

            if (!Regex.IsMatch(model.CCCD ?? "", @"^\d{12}$"))
            {
                ModelState.AddModelError("CCCD", "CCCD phải đúng 12 số");
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(model.Email) || !Regex.IsMatch(model.Email, @"^\S+@\S+\.\S+$"))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ");
                hasError = true;
            }

            if (model.SDT != oldSDT &&
                (_context.KhachHangs.Any(k => k.SDT == model.SDT) ||
                 _context.NhanViens.Any(n => n.SDT == model.SDT)))
            {
                ModelState.AddModelError("SDT", "Số điện thoại đã tồn tại");
                hasError = true;
            }

            if (model.CCCD != oldCCCD &&
                (_context.KhachHangs.Any(k => k.CCCD == model.CCCD) ||
                 _context.NhanViens.Any(n => n.CCCD == model.CCCD)))
            {
                ModelState.AddModelError("CCCD", "CCCD đã tồn tại");
                hasError = true;
            }

            if (model.Email != oldEmail &&
                (_context.KhachHangs.Any(k => k.Email == model.Email) ||
                 _context.NhanViens.Any(n => n.Email == model.Email)))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                hasError = true;
            }

            if (hasError)
            {
                model.TaiKhoan = taiKhoan;
                return View(model);
            }

            if (!string.IsNullOrEmpty(MatKhauCu) || !string.IsNullOrEmpty(MatKhauMoi) || !string.IsNullOrEmpty(XacNhanMatKhauMoi))
            {
                if (taiKhoan.MatKhau != Functions.MD5Password(MatKhauCu ?? ""))
                {
                    ViewBag.ErrorMatKhauCu = "Mật khẩu cũ không đúng";
                    model.TaiKhoan = taiKhoan;
                    return View(model);
                }

                // Mật khẩu mới rỗng
                if (string.IsNullOrEmpty(MatKhauMoi))
                {
                    ViewBag.ErrorMatKhauMoi = "Vui lòng nhập mật khẩu mới";
                    model.TaiKhoan = taiKhoan;
                    return View(model);
                }

                // Xác nhận không trùng
                if (MatKhauMoi != XacNhanMatKhauMoi)
                {
                    ViewBag.ErrorXacNhanMatKhau = "Mật khẩu mới và xác nhận không trùng khớp";
                    model.TaiKhoan = taiKhoan;
                    return View(model);
                }

                taiKhoan.MatKhau = Functions.MD5Password(MatKhauMoi);
                ViewBag.Success = "Cập nhật thông tin và đổi mật khẩu thành công!";
            }
            else
            {
                ViewBag.Success = "Cập nhật thông tin thành công!";
            }

            khachHang.HoTen = model.HoTen;
            khachHang.GioiTinh = model.GioiTinh;
            khachHang.DiaChi = model.DiaChi;
            khachHang.Email = model.Email;
            khachHang.SDT = model.SDT;
            khachHang.CCCD = model.CCCD;

            _context.SaveChanges();

            var updatedKhachHang = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefault(k => k.IDKhachHang == model.IDKhachHang);

            return View(updatedKhachHang);
        }

        public IActionResult Cancel(long id)
        {
            var datDV = _context.DatDVs.Find(id);
            if (datDV == null)
            {
                return NotFound();
            }

            if (datDV.TrangThai != 0)
            {
                return RedirectToAction("Profile", "Users");
            }

            datDV.TrangThai = 3;
            _context.SaveChanges();

            return RedirectToAction("Profile", "Users");
        }
    }
}
