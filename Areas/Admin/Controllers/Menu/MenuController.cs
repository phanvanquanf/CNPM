using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using hotels.Models;
using X.PagedList;
using X.PagedList.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace hotels.Areas.Admin.Controllers;

[Area("Admin")]
public class MenuController : Controller
{
    private readonly DataContext _context;

    public MenuController(DataContext context)
    {
        _context = context;
    }

    private void LoadMenu()
    {
        ViewBag.menuList = (from m in _context.Menus
                            select new SelectListItem()
                            {
                                Text = (m.Levels == 1) ? m.MenuName : "--" + m.MenuName,
                                Value = m.MenuID.ToString(),
                            }).ToList();

    }
    public IActionResult Index(int page = 1)
    {
        LoadMenu();
        var menus = _context.Menus
                            .OrderByDescending(m => m.MenuID)
                            .ToPagedList(page, 8);

        ViewBag.Parents = new SelectList(_context.Menus.Where(m => m.Levels == 1).ToList(), "MenuID", "MenuName");

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_MenuTablePartial", menus);

        return View(menus);
    }

    public IActionResult Create(tblMenu menu)
    {
        if (ModelState.IsValid)
        {
            _context.Menus.Add(menu);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(menu);
    }

    [HttpPost]
    public IActionResult Edit(tblMenu menu)
    {
        LoadMenu();
        if (ModelState.IsValid)
        {
            _context.Menus.Update(menu);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(menu);
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var menu = _context.Menus.Find(id);
        if (menu == null)
            return NotFound();

        _context.Menus.Remove(menu);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}
