using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace hotels.Controllers
{
    public class ServicesController : Controller
    {
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
            return View();
        }

        public IActionResult Airport()
        {
            return View();
        }

        public IActionResult Restaurant()
        {
            return View();
        }

    }
}