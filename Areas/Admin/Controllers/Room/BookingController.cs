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

    public IActionResult Index(int page = 1, string? search = null, int? status = null)
    {
        var query = _context.DatPhongs
                            .Include(dp => dp.CTDatPhongs)
                            .Where(dp => dp.TrangThai == 0 || dp.TrangThai == 1)
                            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(dp => dp.TrangThai == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(dp =>
                _context.KhachHangs.Any(k => k.IDKhachHang == dp.IDKhachHang &&
                    k.HoTen!.ToLower().Contains(search))
            );
        }

        var dp = query
                .OrderBy(dp => dp.TrangThai)
                .ThenByDescending(dp => dp.IDMaDatPhong)
                .ToPagedList(page, 8);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_BookingTablePartial", dp);

        ViewBag.KhachHangs = new SelectList(
            _context.KhachHangs.Where(k => k.TrangThai == 0).Select(k => new { k.IDKhachHang, Display = k.HoTen + " - " + k.SDT }),
            "IDKhachHang", "Display"
        );

        ViewBag.LoaiPhongs = new SelectList(
            _context.LoaiPhongs.Select(l => new { l.IDLoaiPhong, l.LoaiPhong }),
            "IDLoaiPhong", "LoaiPhong"
        );

        ViewBag.Phongs = new SelectList(
            _context.Phongs.Where(p => p.TrangThai == 0)
                .Include(p => p.LoaiPhong)
                .Select(p => new { p.IDPhong, Display = p.MaSoPhong + " - " + p.LoaiPhong!.LoaiPhong }),
            "IDPhong", "Display"
        );

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
    public IActionResult BookingHistory(int page = 1, string? search = null)
    {
        var query = _context.DatPhongs
                            .Include(dp => dp.CTDatPhongs)
                            .Where(dp => dp.TrangThai == 3 || dp.TrangThai == 4)
                            .AsQueryable();

        // Search by customer name only
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(dp =>
                _context.KhachHangs.Any(k => k.IDKhachHang == dp.IDKhachHang &&
                    k.HoTen!.ToLower().Contains(search))
            );
        }

        var dp = query
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


    [HttpGet]
    public IActionResult BookingNew()
    {
        ViewBag.KhachHangs = new SelectList(
            _context.KhachHangs.Where(k => k.TrangThai == 0).Select(k => new { k.IDKhachHang, Display = k.HoTen + " - " + k.SDT }),
            "IDKhachHang", "Display"
        );

        ViewBag.LoaiPhongs = new SelectList(
            _context.LoaiPhongs.Select(l => new { l.IDLoaiPhong, l.LoaiPhong }),
            "IDLoaiPhong", "LoaiPhong"
        );

        ViewBag.Phongs = new SelectList(
            _context.Phongs.Where(p => p.TrangThai == 0)
                .Include(p => p.LoaiPhong)
                .Select(p => new { p.IDPhong, Display = p.MaSoPhong + " - " + p.LoaiPhong!.LoaiPhong }),
            "IDPhong", "Display"
        );

        return View("BookingNew");
    }

    [HttpPost]
    public IActionResult CreateBooking(tblDatPhong model, long IDPhong)
    {
        model.TrangThai = 0;
        model.NgayTao = DateTime.Now;

        _context.DatPhongs.Add(model);
        _context.SaveChanges();

        var ctDatPhong = new tblCTDatPhong
        {
            IDPhong = IDPhong,
            SoKhach = model.TongSoKhach,
            IDMaDatPhong = model.IDMaDatPhong
        };

        _context.CTDatPhongs.Add(ctDatPhong);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult GetPhongsByLoaiPhong(long? idLoaiPhong)
    {
        var query = _context.Phongs
            .Where(p => p.TrangThai == 0)
            .Include(p => p.LoaiPhong)
            .AsQueryable();

        if (idLoaiPhong.HasValue && idLoaiPhong > 0)
        {
            query = query.Where(p => p.IDLoaiPhong == idLoaiPhong);
        }

        var phongs = query.Select(p => new
        {
            idPhong = p.IDPhong,
            maSoPhong = p.MaSoPhong,
            loaiPhong = p.LoaiPhong!.LoaiPhong,
            giaPhong = p.GiaPhong ?? 0
        }).ToList();

        return Json(phongs);
    }
}

