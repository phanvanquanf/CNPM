using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using hotels.Areas.Admin.Models;
using X.PagedList;
using X.PagedList.Extensions;
using hotels.Models;

namespace hotels.Areas.Admin.Controllers;

[Area("Admin")]
public class BookingServicesController : Controller
{
    private readonly DataContext _context;

    public BookingServicesController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Search(string keyword = "", string? status = "", int page = 1)
    {
        var query = _context.DatDVs
                            .Include(dv => dv.DichVu!)
                                .ThenInclude(d => d.LoaiDichVu)
                            .Include(dv => dv.TaiKhoan!.KhachHangs)
                            .AsQueryable();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(dv =>
                (dv.DichVu!.DichVu ?? "").Contains(keyword) || (dv.DichVu!.LoaiDichVu!.LoaiDichVu ?? "").Contains(keyword)
            );
        }

        if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusInt))
        {
            query = query.Where(dv => dv.TrangThai == statusInt);
        }

        query = query.OrderByDescending(dv => dv.IDDatDichVu);

        var pagedList = query.ToPagedList(page, 5);

        if (!pagedList.Any())
        {
            ViewBag.Message = "Không tìm thấy dịch vụ nào phù hợp.";
        }

        return PartialView("_BookingServicePartial", pagedList);
    }

    [HttpGet]
    public IActionResult FilterByStatus(string? status, int page = 1)
    {
        return Search("", status, page);
    }


    public IActionResult Index(int page = 1)
    {
        var datDVs = _context.DatDVs
                             .Include(d => d.DichVu!)
                             .ThenInclude(dv => dv.LoaiDichVu)
                             .OrderBy(d => d.TrangThai)
                             .ThenByDescending(d => d.IDDatDichVu)
                             .ToPagedList(page, 8);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_BookingServicePartial", datDVs);

        return View(datDVs);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.DichVus = new SelectList(_context.DichVus.Where(d => d.TrangThai == 0), "IDDichVu", "DichVu");
        return View();
    }

    [HttpPost]
    public IActionResult Create(tblDatDV model)
    {
        var idTaiKhoan = HttpContext.Session.GetInt32("IDTaiKhoan");
        if (idTaiKhoan == null) return RedirectToAction("Index", "Login", new { area = "" });

        if (!ModelState.IsValid)
        {
            ViewBag.DichVus = new SelectList(_context.DichVus.Where(d => d.TrangThai == 0), "IDDichVu", "DichVu");
            return View(model);
        }

        var dichVu = _context.DichVus.FirstOrDefault(d => d.IDDichVu == model.IDDichVu);
        if (dichVu == null)
        {
            ModelState.AddModelError("", "Dịch vụ không tồn tại.");
            ViewBag.DichVus = new SelectList(_context.DichVus.Where(d => d.TrangThai == 0), "IDDichVu", "DichVu");
            return View(model);
        }

        model.TrangThai = 0;
        if (idTaiKhoan != null)
        {
            model.IDTaiKhoan = idTaiKhoan.Value;
        }

        _context.DatDVs.Add(model);
        _context.SaveChanges();

        return RedirectToAction("BookingSuccess", "Services", new { area = "" });
    }

    [HttpPost]
    public IActionResult Confirm(long id)
    {
        var datDV = _context.DatDVs
                            .Include(d => d.DichVu)
                            .FirstOrDefault(d => d.IDDatDichVu == id);

        if (datDV == null)
            return NotFound();

        if (datDV.TrangThai == 0)
        {
            datDV.TrangThai = 1;
        }
        else if (datDV.TrangThai == 1)
        {
            datDV.TrangThai = 3;
        }

        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    public IActionResult Detail(long id)
    {
        var datDV = _context.DatDVs
                    .Where(d => d.IDDatDichVu == id)
                    .Include(d => d.DichVu!)
                    .ThenInclude(dv => dv.LoaiDichVu)
                    .FirstOrDefault();

        if (datDV == null)
            return NotFound();

        return View(datDV);
    }

    [HttpPost]
    public IActionResult Booking(BookingServiceViewModel model)
    {
        var idTaiKhoan = HttpContext.Session.GetInt32("IDTaiKhoan");
        if (idTaiKhoan == null) return RedirectToAction("Index", "Login", new { area = "" });

        model.IDTaiKhoan = idTaiKhoan.Value;

        foreach (var item in model.SelectedDishes)
        {
            if (item.Value.IsSelected && item.Value.Quantity > 0)
            {
                var dish = _context.DichVus.Find(item.Key);
                if (dish != null)
                {
                    var datDV = new tblDatDV
                    {
                        IDTaiKhoan = model.IDTaiKhoan,
                        IDDichVu = item.Key,
                        NgaySuDung = model.NgaySuDung,
                        SoLuong = item.Value.Quantity,
                        ThanhTien = item.Value.ThanhTien,
                        SoHanhKhach = model.SoHanhKhach,
                        GhiChu = model.GhiChu,
                        TrangThai = 0
                    };
                    _context.DatDVs.Add(datDV);
                }
            }
        }
        _context.SaveChanges();
        return RedirectToAction("BookingSuccess", "Services", new { area = "" });
    }

    [HttpPost]
    public IActionResult BookingSpa(BookingServiceViewModel model)
    {
        var idTaiKhoan = HttpContext.Session.GetInt32("IDTaiKhoan");
        if (idTaiKhoan == null)
            return RedirectToAction("Index", "Login", new { area = "" });

        model.IDTaiKhoan = idTaiKhoan.Value;

        foreach (var item in model.SelectedDishes)
        {
            if (item.Value.IsSelected && item.Value.Quantity > 0)
            {
                var spa = _context.DichVus.Find(item.Key);
                if (spa != null)
                {
                    var datDV = new tblDatDV
                    {
                        IDTaiKhoan = model.IDTaiKhoan,
                        IDDichVu = item.Key,
                        NgaySuDung = DateTime.Parse($"{model.NgaySuDung:yyyy-MM-dd} {model.GioSuDung}"),
                        SoLuong = item.Value.Quantity,
                        ThanhTien = item.Value.ThanhTien,
                        GhiChu = model.GhiChu,
                        TrangThai = 0
                    };

                    _context.DatDVs.Add(datDV);
                }
            }
        }
        _context.SaveChanges();
        return RedirectToAction("BookingSuccess", "Services", new { area = "" });
    }
}

