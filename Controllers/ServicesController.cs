using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hotels.Models;
using Microsoft.AspNetCore.Mvc;

namespace hotels.Controllers
{
    public class ServicesController : Controller
    {
        private readonly DataContext _context;

        public ServicesController(DataContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Events()
        {
            return View();
        }

        public IActionResult Spa()
        {
            var spaOffers = _context.DichVus
                         .Where(d => d.TrangThai == 0 && d.IDLoaiDichVu == 2)
                         .OrderBy(d => d.DichVu)
                         .ToList();

            return View(spaOffers);
        }

        public IActionResult Airport()
        {
            var cars = _context.DichVus
            .Where(d => d.TrangThai == 0 && d.IDLoaiDichVu == 3)
            .OrderBy(d => d.DichVu)
            .ToList();

            return View(cars);
        }

        public IActionResult Restaurant()
        {
            var dishes = _context.DichVus
                        .Where(d => d.IDLoaiDichVu == 1 && d.TrangThai == 0)
                        .OrderBy(d => d.DichVu)
                        .ToList();

            return View(dishes);
        }

        public IActionResult BookingSuccess()
        {
            return View();
        }
    }
}