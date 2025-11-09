using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hotels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotels.Controllers
{
    public class RoomController : Controller
    {
        private readonly DataContext _context;

        public RoomController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var listPhong = _context.Phongs
            .Include(p => p.LoaiPhong!)
                .ThenInclude(l => l.TienNghis!)
            .Include(p => p.AnhPhongs).ToList();

            ViewBag.Title = "Danh sách phòng - Khách sạn Nghệ An";

            return View(listPhong);
        }

        public IActionResult Details(long id)
        {
            var phong = _context.Phongs
            .Include(p => p.LoaiPhong!)
                .ThenInclude(l => l.TienNghis!)
            .Include(p => p.AnhPhongs)
            .FirstOrDefault(p => p.IDPhong == id);

            ViewBag.Title = "Chi tiết phòng " + phong?.MaSoPhong;
            if (phong == null)
                return NotFound();

            return View(phong);
        }

        [HttpGet]
        public IActionResult Booking(int? type)
        {
            var loaiPhongs = _context.LoaiPhongs.ToList();
            var idTaiKhoan = HttpContext.Session.GetInt32("IDTaiKhoan");

            if (idTaiKhoan == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.LoaiPhongs = loaiPhongs;
            ViewBag.SelectedLoaiPhong = type;

            return View();
        }


        [HttpPost]
        public IActionResult BookingPost(tblDatPhong dp, long LoaiPhong)
        {
            _context.DatPhongs.Add(dp);
            _context.SaveChanges();

            var phong = _context.Phongs.FirstOrDefault(p => p.IDLoaiPhong == LoaiPhong);

            if (phong != null)
            {
                var ct = new tblCTDatPhong
                {
                    IDPhong = phong.IDPhong,
                    SoKhach = dp.TongSoKhach,
                    IDMaDatPhong = dp.IDMaDatPhong
                };

                _context.CTDatPhongs.Add(ct);
                _context.SaveChanges();
            }

            return RedirectToAction("BookingSuccess");
        }


        public IActionResult BookingSuccess()
        {
            return View();
        }
    }
}