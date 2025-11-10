using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using hotels.Models;
using X.PagedList.Extensions;
using hotels.Areas.Admin.Models;

namespace hotels.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminMenuController : Controller
{
    private readonly DataContext _context;

    public AdminMenuController(DataContext context)
    {
        _context = context;
    }

    public IActionResult Index(int page = 1)
    {
        var menus = _context.AdminMenus
                            .OrderByDescending(m => m.AdminMenuID)
                            .ToPagedList(page, 8);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_AdminMenuTablePartial", menus);

        return View(menus);
    }

    [HttpPost]
    public IActionResult Create(AdminMenu adminMenu)
    {
        if (ModelState.IsValid)
        {
            _context.AdminMenus.Add(adminMenu);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        return View(adminMenu);
    }


    [HttpPost]
    public IActionResult Delete(long id)
    {
        var menu = _context.AdminMenus.Find(id);
        if (menu == null)
            return NotFound();

        _context.AdminMenus.Remove(menu);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}
