using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hotels.Areas.Admin.Models;
using hotels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace hotels.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly DataContext _context;

        public TaiKhoanController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Search(string keyword = "", string? role = "", int page = 1)
        {
            var query = _context.TaiKhoans.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(tk => (tk.TenDangNhap ?? "").Contains(keyword));
            }

            if (!string.IsNullOrEmpty(role) && int.TryParse(role, out int roleInt))
            {
                query = query.Where(tk => tk.VaiTro == roleInt);
            }

            query = query.OrderByDescending(tk => tk.IDTaiKhoan);

            var pagedList = query.ToPagedList(page, 3);

            if (!pagedList.Any())
            {
                ViewBag.Message = "Không tìm thấy tài khoản nào phù hợp.";
            }

            return PartialView("_TaiKhoanPartial", pagedList);
        }


        [HttpGet]
        public IActionResult FilterByRole(string? role, int page = 1)
        {
            return Search("", role, page);
        }

        public IActionResult Index(int page = 1)
        {
            var TKList = _context.TaiKhoans.OrderByDescending(p => p.IDTaiKhoan).ToPagedList(page, 3);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_TaiKhoanPartial", TKList);

            return View(TKList);
        }

        [HttpPost]
        public JsonResult KiemTraTenDangNhap(string tenDangNhap)
        {
            var exists = _context.TaiKhoans.Any(t => t.TenDangNhap == tenDangNhap);
            return Json(new { exists });
        }

        [HttpPost]
        public IActionResult Create(tblTaiKhoan tk, IFormFile? Image)
        {

            string folderName = tk.VaiTro switch
            {
                0 => "Admin",
                1 => "KH",
                2 => "NV",
                _ => ""
            };

            string folderPath = string.IsNullOrEmpty(folderName)
                ? Path.Combine("wwwroot", "assets", "img", "AnhDaiDien")
                : Path.Combine("wwwroot", "assets", "img", "AnhDaiDien", folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            if (Image != null && Image.Length > 0)
            {
                string extension = Path.GetExtension(Image.FileName);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(Image.FileName);
                string fileName = $"{fileNameWithoutExt}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Image.CopyTo(stream);
                }

                tk.Image = fileName;
            }
            else
            {
                tk.Image = "default.jpg";
            }

            tk.NgayTao = DateTime.Now;

            _context.TaiKhoans.Add(tk);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(tblTaiKhoan tk, IFormFile? Image, string? OldImage)
        {
            var existing = _context.TaiKhoans.FirstOrDefault(x => x.IDTaiKhoan == tk.IDTaiKhoan);
            if (existing == null)
                return NotFound();

            string folderName = tk.VaiTro switch
            {
                0 => "Admin",
                1 => "KH",
                2 => "NV",
                _ => ""
            };

            string folderPath = string.IsNullOrEmpty(folderName)
                ? Path.Combine("wwwroot", "assets", "img", "AnhDaiDien")
                : Path.Combine("wwwroot", "assets", "img", "AnhDaiDien", folderName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // 📸 Xử lý ảnh
            if (Image != null && Image.Length > 0)
            {
                string extension = Path.GetExtension(Image.FileName);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(Image.FileName);
                string fileName = $"{fileNameWithoutExt}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Image.CopyTo(stream);
                }

                existing.Image = fileName;
            }
            else
            {
                existing.Image = OldImage ?? existing.Image;
            }

            existing.TenDangNhap = tk.TenDangNhap;
            existing.MatKhau = tk.MatKhau;
            existing.VaiTro = tk.VaiTro;
            existing.TrangThai = tk.TrangThai;
            existing.NgayTao = tk.NgayTao;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Delete(long id)
        {
            var taikhoan = _context.TaiKhoans
                            .Include(p => p.NhanViens)
                            .Include(p => p.KhachHangs)
                            .FirstOrDefault(p => p.IDTaiKhoan == id);

            if (taikhoan == null)
                return NotFound();

            if (taikhoan.NhanViens != null && taikhoan.NhanViens.Any())
            {
                foreach (var tk in taikhoan.NhanViens)
                {
                    tk.IDTaiKhoan = null;
                }
            }

            if (taikhoan.KhachHangs != null && taikhoan.KhachHangs.Any())
            {
                foreach (var tk in taikhoan.KhachHangs)
                {
                    tk.IDTaiKhoan = null;
                }
            }

            _context.TaiKhoans.Remove(taikhoan);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}