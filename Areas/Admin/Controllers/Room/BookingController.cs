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
}
