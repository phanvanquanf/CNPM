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
        public IActionResult CheckUnique(string sdt, string cccd, string email, long idTaiKhoan)
        {
            bool sdtExists = _context.NhanViens.Any(x => x.SDT == sdt);
            bool cccdExists = _context.NhanViens.Any(x => x.CCCD == cccd);
            bool emailExists = _context.NhanViens.Any(x => x.Email == email);
            bool taiKhoanExists = _context.NhanViens.Any(x => x.IDTaiKhoan == idTaiKhoan);

            return Ok(new { sdtExists, cccdExists, emailExists, taiKhoanExists });
        }

        [HttpGet]
        public IActionResult Search(string keyword = "", string? status = "", int page = 1)
        {
            ViewBag.TaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IDTaiKhoan", "TenDangNhap");
            var query = _context.NhanViens
                                .Include(nv => nv.TaiKhoan)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(nv => (nv.HoTen ?? "").Contains(keyword) ||
                                          (nv.Email ?? "").Contains(keyword) ||
                                          (nv.SDT ?? "").Contains(keyword));
            }

            if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusInt))
            {
                query = query.Where(nv => nv.TrangThai == statusInt);
            }

            query = query.OrderByDescending(nv => nv.IDNhanVien);

            var pagedList = query.ToPagedList(page, 3);

            if (!pagedList.Any())
            {
                ViewBag.Message = "Không tìm thấy nhân viên nào phù hợp.";
            }

            return PartialView("_NhanVienTablePartial", pagedList);
        }


        [HttpGet]
        public IActionResult FilterByStatus(string? status, int page = 1)
        {
            return Search("", status, page);
        }


        public IActionResult Index(int page = 1)
        {

            ViewBag.TaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IDTaiKhoan", "TenDangNhap");
            var nvList = _context.NhanViens.Include(m => m.TaiKhoan)
                                .OrderByDescending(m => m.IDNhanVien)
                                .ToPagedList(page, 3);

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

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.NhanViens.Update(nv);
            _context.SaveChanges();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var nvList = _context.NhanViens.Include(m => m.TaiKhoan)
                                              .OrderByDescending(m => m.IDNhanVien)
                                              .ToPagedList(1, 3);
                return PartialView("_NhanVienTablePartial", nvList);
            }

            return RedirectToAction("Index");
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