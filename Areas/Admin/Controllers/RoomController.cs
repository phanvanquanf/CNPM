using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hotels.Models;
using hotels.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace hotels.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomController : Controller
    {
        private readonly DataContext _context;

        public RoomController(DataContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Search(string keyword = "", string? status = "", int page = 1)
        {
            var query = _context.Phongs
                                .Include(p => p.LoaiPhong)
                                .Include(p => p.AnhPhongs)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => (p.MaSoPhong ?? "").Contains(keyword));
            }

            if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusInt))
            {
                query = query.Where(p => p.TrangThai == statusInt);
            }

            query = query.OrderByDescending(p => p.IDPhong);

            var pagedList = query.ToPagedList(page, 3);

            if (!pagedList.Any())
            {
                ViewBag.Message = "Không tìm thấy phòng nào phù hợp.";
            }

            return PartialView("_RoomPartial", pagedList);
        }

        [HttpGet]
        public IActionResult FilterByStatus(string? status, int page = 1)
        {
            return Search("", status, page);
        }

        public IActionResult Index(int page = 1)
        {
            var phongList = _context.Phongs.Include(p => p.LoaiPhong)
                                    .Include(p => p.AnhPhongs)
                                    .OrderByDescending(p => p.IDPhong)
                                    .ToPagedList(page, 3);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_RoomPartial", phongList);
            ViewBag.LoaiPhong = new SelectList(_context.LoaiPhongs.ToList(), "IDLoaiPhong", "LoaiPhong");
            return View(phongList);
        }

        [HttpPost]
        public IActionResult Create(tblPhong phong, string AnhChinhLink, List<string> AnhPhuLinks)
        {
            ViewBag.LoaiPhong = new SelectList(_context.LoaiPhongs.ToList(), "IDLoaiPhong", "LoaiPhong");

            _context.Phongs.Add(phong);
            _context.SaveChanges();

            var anhPhongs = new List<tblAnhPhong>();

            if (!string.IsNullOrEmpty(AnhChinhLink))
            {
                anhPhongs.Add(new tblAnhPhong
                {
                    IDPhong = phong.IDPhong,
                    Link = AnhChinhLink,
                    AnhChinh = true
                });
            }

            if (AnhPhuLinks != null && AnhPhuLinks.Any())
            {
                foreach (var link in AnhPhuLinks)
                {
                    if (string.IsNullOrEmpty(link)) continue;

                    anhPhongs.Add(new tblAnhPhong
                    {
                        IDPhong = phong.IDPhong,
                        Link = link,
                        AnhChinh = false
                    });
                }
            }

            if (anhPhongs.Any())
            {
                _context.AnhPhongs.AddRange(anhPhongs);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(tblPhong phong, IFormFile AnhChinh, List<IFormFile> AnhPhuFiles)
        {
            var listPhong = _context.Phongs
                .Include(p => p.AnhPhongs)
                .FirstOrDefault(p => p.IDPhong == phong.IDPhong);

            if (listPhong == null)
                return NotFound();

            listPhong.MaSoPhong = phong.MaSoPhong;
            listPhong.ViTri = phong.ViTri;
            listPhong.GiaPhong = phong.GiaPhong;
            listPhong.TongQuan = phong.TongQuan;
            listPhong.Rating = phong.Rating;
            listPhong.Reviewer = phong.Reviewer;
            listPhong.IDLoaiPhong = phong.IDLoaiPhong;
            listPhong.TrangThai = phong.TrangThai;

            if (AnhChinh != null && phong.MaSoPhong != null)
            {
                var folder = Path.Combine("wwwroot/assets/img/Room", phong.MaSoPhong);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Path.GetFileName(AnhChinh.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    AnhChinh.CopyTo(stream);
                }

                var relativePath = $"assets/img/Room/{phong.MaSoPhong}/{fileName}";

                var anhChinh = listPhong.AnhPhongs?.FirstOrDefault(a => a.AnhChinh);
                if (anhChinh != null)
                    anhChinh.Link = relativePath;
                else
                    listPhong.AnhPhongs?.Add(new tblAnhPhong
                    {
                        IDPhong = listPhong.IDPhong,
                        Link = relativePath,
                        AnhChinh = true
                    });
            }

            if (phong.MaSoPhong != null)
            {
                var folder = Path.Combine("wwwroot/assets/img/Room", phong.MaSoPhong);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var existingPhu = listPhong.AnhPhongs?.Where(a => !a.AnhChinh).ToList() ?? new List<tblAnhPhong>();

                if (AnhPhuFiles != null && AnhPhuFiles.Count > 0)
                {
                    for (int i = 0; i < AnhPhuFiles.Count; i++)
                    {
                        var file = AnhPhuFiles[i];
                        if (file == null) continue;

                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }

                        var relativePath = $"assets/img/Room/{phong.MaSoPhong}/{fileName}";

                        if (i < existingPhu.Count)
                        {
                            var anh = existingPhu[i];
                            anh.Link = relativePath;
                            _context.Entry(anh).State = EntityState.Modified;
                        }
                        else
                        {
                            listPhong.AnhPhongs?.Add(new tblAnhPhong
                            {
                                IDPhong = phong.IDPhong,
                                Link = relativePath,
                                AnhChinh = false
                            });
                        }
                    }
                }
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }



        [HttpPost]
        public IActionResult Delete(long id)
        {
            var phong = _context.Phongs
                        .Include(p => p.AnhPhongs)
                        .FirstOrDefault(p => p.IDPhong == id);

            if (phong == null)
                return NotFound();

            if (phong.AnhPhongs != null && phong.AnhPhongs.Any())
            {
                _context.AnhPhongs.RemoveRange(phong.AnhPhongs);
            }

            _context.Phongs.Remove(phong);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}