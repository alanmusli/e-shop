using System.Diagnostics;
using e_store.Data;
using Microsoft.AspNetCore.Mvc;
using e_store.Models;
using Microsoft.EntityFrameworkCore;

namespace e_store.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var brands = _context.Brands.ToList();

        var categories = _context.Categories.Include(x=>x.SubCategories).ToList();
        var model = new CategoryBrandViewModel()
        {
            Brands = brands,
            Categories = categories
        };
            
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}