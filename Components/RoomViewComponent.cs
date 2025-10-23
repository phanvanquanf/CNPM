using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hotels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hotels.Components
{
    [ViewComponent(Name = "RoomView")]
    public class RoomViewComponent : ViewComponent
    {
        private readonly DataContext _context;

        public RoomViewComponent(DataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var listOfRoom = (from m in _context.Phongs.Include(p => p.AnhPhongs).Include(p => p.LoaiPhong)
                              where m.TrangThai == 0
                              select m).ToList();
            return await Task.FromResult((IViewComponentResult)View("Room", listOfRoom));
        }
    }
}