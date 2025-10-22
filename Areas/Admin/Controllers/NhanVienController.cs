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
    public class NhanVienController : Controller
    {
        private readonly DataContext _context;
        public NhanVienController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Search(string keyword)
        {
            var list = _context.NhanViens.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                list = list.Where(nv => (nv.HoTen ?? "").Contains(keyword) ||
                                        (nv.Email ?? "").Contains(keyword) ||
                                        (nv.SDT ?? "").Contains(keyword));
            }

            var result = list.ToList();

            if (!result.Any())
            {
                ViewBag.Message = "Không tìm thấy nhân viên nào phù hợp.";
            }

            var pagedList = result.ToPagedList(1, result.Count == 0 ? 1 : result.Count);
            return PartialView("_NhanVienTablePartial", pagedList);
        }

        [HttpGet]
        public IActionResult FilterByStatus(string? status)
        {
            var query = _context.NhanViens.Include(nv => nv.TaiKhoan).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(nv => nv.TrangThai.ToString() == status);
            }

            var result = query.OrderByDescending(nv => nv.IDNhanVien).ToPagedList(1, 8);
            return PartialView("_NhanVienTablePartial", result);
        }

        public IActionResult Index(int page = 1)
        {

            ViewBag.TaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IDTaiKhoan", "TenDangNhap");
            var nvList = _context.NhanViens.Include(m => m.TaiKhoan)
                                .OrderByDescending(m => m.IDNhanVien)
                                .ToPagedList(page, 8);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_NhanVienTablePartial", nvList);

            return View(nvList);
        }

        [HttpPost]
        public IActionResult Create(tblNhanVien nv)
        {
            if (ModelState.IsValid)
            {
                _context.NhanViens.Add(nv);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IDTaiKhoan", "TenDangNhap");
            return View(nv);
        }

        [HttpPost]
        public IActionResult Edit(tblNhanVien nv)
        {
            ViewBag.TaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IDTaiKhoan", "TenDangNhap");
            if (ModelState.IsValid)
            {
                _context.NhanViens.Update(nv);
                _context.SaveChanges();
                return RedirectToAction("Index");

            }

            return View(nv);
        }

        [HttpPost]
        public IActionResult Delete(long id)
        {
            var nv = _context.NhanViens.Find(id);
            if (nv == null)
                return NotFound();
            _context.NhanViens.Remove(nv);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}