using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using hotels.Models;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace hotels.Areas.Admin.Controllers;

[Area("Admin")]
public class BookingController : Controller
{
    private readonly DataContext _context;

    public BookingController(DataContext context)
    {
        _context = context;
    }

    public IActionResult Index(int page = 1)
    {
        var dp = _context.DatPhongs
                            .Where(dp => dp.TrangThai == 0 || dp.TrangThai == 1)
                            .OrderBy(dp => dp.TrangThai)
                            .ThenByDescending(dp => dp.IDMaDatPhong)
                            .ToPagedList(page, 8);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_BookingTablePartial", dp);

        return View(dp);
    }

    [HttpPost]
    public IActionResult Confirm(long id, int trangThaiMoi)
    {
        var idKhachHang = HttpContext.Session.GetInt32("IDKhachHang");
        var dp = _context.DatPhongs.Find(id);
        if (dp == null)
        {
            return NotFound();
        }

        dp.TrangThai = trangThaiMoi;

        if (idKhachHang != null)
            dp.IDKhachHang = idKhachHang.Value;

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ConfirmClient(long id, int trangThaiMoi)
    {
        var idKhachHang = HttpContext.Session.GetInt32("IDKhachHang");
        var dp = _context.DatPhongs.Find(id);
        if (dp == null)
        {
            return NotFound();
        }

        dp.TrangThai = trangThaiMoi;

        if (idKhachHang != null)
            dp.IDKhachHang = idKhachHang.Value;

        _context.SaveChanges();
        return RedirectToAction("Profile", "Users", new { area = "" });
    }

    [HttpGet]
    public IActionResult BookingHistory(int page = 1)
    {
        var dp = _context.DatPhongs
        .Where(dp => dp.TrangThai == 3 || dp.TrangThai == 4)
                .OrderByDescending(dp => dp.IDMaDatPhong)
                    .ToPagedList(page, 8);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_BookingHistoryPartial", dp);

        return View(dp);
    }

    public IActionResult Detail(long id)
    {
        var booking = _context.DatPhongs
            .Where(x => x.IDMaDatPhong == id)
            .FirstOrDefault();

        if (booking == null)
            return NotFound();

        var details = _context.CTDatPhongs
            .Where(x => x.IDMaDatPhong == id)
            .Include(d => d.Phong!)
            .ThenInclude(p => p.AnhPhongs)
            .Include(d => d.Phong!)
            .ThenInclude(p => p.LoaiPhong)
            .ToList();

        var customer = _context.KhachHangs
            .FirstOrDefault(k => k.IDKhachHang == booking.IDKhachHang);

        ViewBag.Details = details;
        ViewBag.Customer = customer;

        return PartialView("_BookingDetail", booking);
    }


    [HttpPost]
    public IActionResult DeleteBookingHistory(long id)
    {
        var listCT = _context.CTDatPhongs
            .Where(x => x.IDDatPhong == id)
            .ToList();

        var dp = _context.DatPhongs.Find(id);

        if (dp == null)
            return NotFound();

        _context.CTDatPhongs.RemoveRange(listCT);
        _context.DatPhongs.Remove(dp);

        _context.SaveChanges();

        return RedirectToAction("BookingHistory");
    }

    [HttpPost]
    public IActionResult DelBookingHistory(long id)
    {
        var listCT = _context.CTDatPhongs
            .Where(x => x.IDDatPhong == id)
            .ToList();

        var dp = _context.DatPhongs.Find(id);

        if (dp == null)
            return NotFound();

        _context.CTDatPhongs.RemoveRange(listCT);
        _context.DatPhongs.Remove(dp);

        _context.SaveChanges();

        return RedirectToAction("Profile", "Users", new { area = "" });
    }
}
