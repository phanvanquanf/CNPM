using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using hotels.Areas.Admin.Models;
using hotels.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace hotels.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhachHangController : Controller
    {
        private readonly DataContext _context;
        public KhachHangController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Search(string keyword = "", string? status = "", int page = 1)
        {
            ViewBag.TaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IDTaiKhoan", "TenDangNhap");
            var query = _context.KhachHangs
                                .Include(kh => kh.TaiKhoan)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(kh => (kh.HoTen ?? "").Contains(keyword) ||
                                        (kh.Email ?? "").Contains(keyword) ||
                                        (kh.SDT ?? "").Contains(keyword));
            }

            if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusInt))
            {
                query = query.Where(kh => kh.TrangThai == statusInt);
            }

            query = query.OrderByDescending(kh => kh.IDKhachHang);
            var pagedList = query.ToPagedList(page, 3);

            if (!pagedList.Any())
            {
                ViewBag.Message = "Không tìm thấy khách hàng nào phù hợp.";
            }

            return PartialView("_KhachHangTablePartial", pagedList);
        }



        [HttpGet]
        public IActionResult FilterByStatus(string? status, int page = 1)
        {
            return Search("", status, page);
        }


        public IActionResult Index(int page = 1)
        {

            ViewBag.TaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IDTaiKhoan", "TenDangNhap");
            var khList = _context.KhachHangs.Include(m => m.TaiKhoan)
                                .OrderByDescending(m => m.IDKhachHang)
                                .ToPagedList(page, 3);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_KhachHangTablePartial", khList);

            return View(khList);
        }

        [HttpPost]
        public IActionResult Delete(long id)
        {
            var kh = _context.KhachHangs.Find(id);
            if (kh == null)
                return NotFound();
            _context.KhachHangs.Remove(kh);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}