using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using hotels.Models;
using X.PagedList;
using System.Linq;
using X.PagedList.Extensions;
using hotels.Areas.Admin.Models;

namespace hotels.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServicesTypesController : Controller
    {
        private readonly DataContext _context;

        public ServicesTypesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Search(string keyword = "", string? status = "", int page = 1)
        {
            var query = _context.LoaiDichVus.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(ldv =>
                    (ldv.LoaiDichVu ?? "").Contains(keyword) ||
                    (ldv.MoTa ?? "").Contains(keyword));
            }

            if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusInt))
            {
                query = query.Where(ldv => ldv.TrangThai == statusInt);
            }

            query = query.OrderByDescending(ldv => ldv.IDLoaiDichVu);

            var pagedList = query.ToPagedList(page, 5);

            if (!pagedList.Any())
            {
                ViewBag.Message = "Không tìm thấy loại dịch vụ nào phù hợp.";
            }

            return PartialView("_servicesTypes", pagedList);
        }

        [HttpGet]
        public IActionResult FilterByStatus(string? status, int page = 1)
        {
            return Search("", status, page);
        }


        public IActionResult Index(int page = 1)
        {
            var serviceTypes = _context.LoaiDichVus
                                       .OrderByDescending(s => s.IDLoaiDichVu)
                                       .ToPagedList(page, 8);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_servicesTypes", serviceTypes);

            return View(serviceTypes);
        }

        [HttpPost]
        public IActionResult Create(tblLoaiDichVu model)
        {
            if (ModelState.IsValid)
            {
                _context.LoaiDichVus.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }


            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(tblLoaiDichVu model)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.LoaiDichVus.Find(model.IDLoaiDichVu);
                if (existing == null) return NotFound();

                existing.LoaiDichVu = model.LoaiDichVu;
                existing.MoTa = model.MoTa;
                existing.TrangThai = model.TrangThai;

                _context.LoaiDichVus.Update(existing);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public IActionResult Delete(long id)
        {
            var serviceType = _context.LoaiDichVus.Find(id);
            if (serviceType == null) return NotFound();

            _context.LoaiDichVus.Remove(serviceType);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
