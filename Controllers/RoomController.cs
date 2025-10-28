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
        public IActionResult Booking()
        {
            return View();
        }
    }
}