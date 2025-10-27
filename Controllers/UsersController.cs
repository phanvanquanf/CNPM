using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hotels.Models;

namespace hotels.Controllers
{
    public class UsersController : Controller
    {
        private readonly DataContext _context;

        public UsersController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Profile()
        {
            long userId = 1;

            var datPhongs = _context.DatPhongs
            .Include(dp => dp.CTDatPhongs!)
                .ThenInclude(ctdp => ctdp.Phong!)
                    .ThenInclude(p => p.LoaiPhong!)
            .Include(dp => dp.CTDatPhongs!)
                .ThenInclude(ctdp => ctdp.Phong!)
                    .ThenInclude(p => p.AnhPhongs!)
            .Where(dp => dp.IDKhachHang == userId)
            .OrderByDescending(dp => dp.NgayTao)
            .ToList();

            var dpGanNhat = datPhongs.FirstOrDefault();

            ViewBag.dpGanNhat = dpGanNhat;
            return View(datPhongs);
        }
    }
}
