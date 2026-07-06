using e_store.Data;
using e_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e_store.Controllers;

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
    
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var brand = await _context.Brands
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (brand == null)
        {
            return NotFound();
        }

        return View(brand);
    }
    
}
