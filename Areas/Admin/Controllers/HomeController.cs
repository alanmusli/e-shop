using System.Diagnostics;
using e_store.Areas.Admin.Models.ViewModels;
using e_store.Data;
using Microsoft.AspNetCore.Mvc;
using e_store.Models;
using Microsoft.AspNetCore.Authorization;

namespace e_store.Areas.Admin.Controllers;

[Area("Admin")] 
[Authorize(Roles = "Admin")]
[Route("Admin/[controller]/[action]")] 
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("~/Admin")]
    public IActionResult Index()
    {
        var allOrders = _context.Orders.Count();
        var notProccessed = _context.Orders.Where(x => x.OrderStatus != "delivered").ToList();
        var productCount = _context.Products.Count();
        var lowCountProds = _context.Products.Where(x => x.InventoryCount < 3).Count();

        var totalSale = _context.Orders.Where(x => x.OrderStatus == "delivered").Sum(x=>x.TotalAmount);
        var messages = _context.ContactMessages.Where(x=>!x.isRead).Count();


        var info = new HomeInfo()
        {
            allOrders = allOrders,
            notProccessed = notProccessed,
            productCount = productCount,
            lowCountProds = lowCountProds,
            totalSale = totalSale,
            messages = messages

        };
        
        return View(info);
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