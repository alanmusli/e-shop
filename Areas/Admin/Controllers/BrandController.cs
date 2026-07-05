using e_store.Data;
using e_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e_store.Areas.Admin.Controllers;

[Area("Admin")] 
[Authorize(Roles = "Admin")]
public class BrandController : Controller
{
    private readonly ApplicationDbContext _context;
    public BrandController (ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View(_context.Brands.ToList());
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View(new Brand(){});
    }

    [HttpPost]
    public IActionResult Add(int Id,string? Name)
    {
        var isThere = _context.Brands.FirstOrDefault(x => x.Name.ToLower() == Name.ToLower());
        if (isThere == null)
        {
            var brand = new Brand()
            {
                Id = Id,
                Name = Name
            };

            _context.Brands.Add(brand);
            _context.SaveChanges();
        }

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // 2. Add 'await' and change '.FirstOrDefault()' to '.FirstOrDefaultAsync()'
        var brand = await _context.Brands
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (brand == null)
        {
            return NotFound();
        }

        return View(brand);
    }
    
    [HttpPost]
    public IActionResult Remove(int id)
    {
        
        bool hasProducts = _context.Products.Any(p => p.BrandId == id);
    
        if (hasProducts)
        {
            TempData["ErrorMessage"] = "Не можете да го избришете овој бренд бидејќи има продукти поврзани со него. Прво избришете ги продуктите.";
            return RedirectToAction("Index", "Brand");
        }
        
        var brand = _context.Brands.FirstOrDefault(x => x.Id == id);

        if (brand != null)
        {
            _context.Brands.Remove(brand);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Брендот е успешно избришан.";
        }

        return RedirectToAction("Index", "Brand");
    }
}
