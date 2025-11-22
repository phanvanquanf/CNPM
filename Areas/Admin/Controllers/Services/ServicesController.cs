using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using hotels.Areas.Admin.Models;
using hotels.Models;

namespace hotels.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServicesController : Controller
    {
        private readonly DataContext _context;

        public ServicesController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1)
        {
            ViewBag.LoaiDichVu = new SelectList(_context.LoaiDichVus.ToList(), "IDLoaiDichVu", "LoaiDichVu");

            var dvList = _context.DichVus
                                 .Include(dv => dv.LoaiDichVu)
                                 .OrderByDescending(dv => dv.IDDichVu)
                                 .ToPagedList(page, 5);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_ServicesPartial", dvList);

            return View(dvList);
        }

        [HttpGet]
        public IActionResult Search(string keyword = "", string? status = "", int page = 1)
        {
            ViewBag.LoaiDichVu = new SelectList(_context.LoaiDichVus.ToList(), "IDLoaiDichVu", "LoaiDichVu");

            var query = _context.DichVus
                                .Include(dv => dv.LoaiDichVu)
                                .OrderByDescending(dv => dv.IDDichVu)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(dv =>
                    (dv.DichVu ?? "").Contains(keyword) ||
                    (dv.MoTa ?? "").Contains(keyword) ||
                    (dv.LoaiDichVu!.LoaiDichVu ?? "").Contains(keyword));
            }

            if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusInt))
            {
                query = query.Where(dv => dv.TrangThai == statusInt);
            }

            query = query.OrderByDescending(dv => dv.IDDichVu);
            var pagedList = query.ToPagedList(page, 5);

            if (!pagedList.Any())
            {
                ViewBag.Message = "Không tìm thấy dịch vụ nào phù hợp.";
            }

            return PartialView("_ServicesPartial", pagedList);
        }

        [HttpGet]
        public IActionResult FilterByStatus(string? status, int page = 1)
        {

            return Search("", status, page);
        }

        [HttpPost]
        public IActionResult Create(tblDichVu model, IFormFile? Image)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.LoaiDichVu = new SelectList(_context.LoaiDichVus, "IDLoaiDichVu", "LoaiDichVu");
                return View(model);
            }

            var loaiDV = _context.LoaiDichVus.FirstOrDefault(l => l.IDLoaiDichVu == model.IDLoaiDichVu);
            string? loaiDVName = loaiDV?.LoaiDichVu;

            _context.DichVus.Add(model);
            _context.SaveChanges();

            // 2. Upload ảnh
            if (Image != null && Image.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/assets/img/services/{loaiDVName}");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Image.CopyTo(stream);
                }

                // Lưu đường dẫn tương đối trong DB
                model.Image = fileName;
                _context.DichVus.Update(model);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }



        [HttpPost]
        public IActionResult Edit(tblDichVu model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.LoaiDichVu = new SelectList(_context.LoaiDichVus, "IDLoaiDichVu", "LoaiDichVu");
                return View(model);
            }

            var loaiDV = _context.LoaiDichVus.FirstOrDefault(l => l.IDLoaiDichVu == model.IDLoaiDichVu);
            string? loaiDVName = loaiDV?.LoaiDichVu;


            var existing = _context.DichVus.FirstOrDefault(d => d.IDDichVu == model.IDDichVu);
            if (existing == null) return RedirectToAction("Index");

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/assets/img/services/{loaiDVName}");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ImageFile.CopyTo(stream);
                }

                existing.Image = fileName;
            }

            existing.DichVu = model.DichVu;
            existing.GiaDichVu = model.GiaDichVu;
            existing.GiaUuDai = model.GiaUuDai;
            existing.ThoiLuong = model.ThoiLuong;
            existing.DacDiem = model.DacDiem;
            existing.MoTa = model.MoTa;
            existing.TrangThai = model.TrangThai;
            existing.IDLoaiDichVu = model.IDLoaiDichVu;

            _context.DichVus.Update(existing);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(long id)
        {
            var service = _context.DichVus.Find(id);
            if (service == null)
                return NotFound();

            _context.DichVus.Remove(service);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
