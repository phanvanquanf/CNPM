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
        var dp = _context.DatPhongs.Find(id);
        if (dp == null)
        {
            return NotFound();
        }

        dp.TrangThai = trangThaiMoi;

        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    // BookingController.cs
        [HttpGet]
        public IActionResult Detail(long id)
        {
            var booking = _context.DatPhongs
                .Include(dp => dp.CTDatPhongs!)
                    .ThenInclude(ctdp => ctdp.Phong!)
                        .ThenInclude(p => p.LoaiPhong!)
                .Include(dp => dp.CTDatPhongs!)
                    .ThenInclude(ctdp => ctdp.Phong!)
                        .ThenInclude(p => p.AnhPhongs!)
                .FirstOrDefault(dp => dp.IDMaDatPhong == id);

            if (booking == null)
                return NotFound();

            return PartialView("_BookingDetail", booking);
        }



}
