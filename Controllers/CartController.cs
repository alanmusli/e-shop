using e_store.Data;
using e_store.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e_store.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public IActionResult Index()
    {
        var userId = _userManager.GetUserId(User);
        var cart = _context.Carts.Include(x=>x.CartItems).ThenInclude(x=>x.Product).FirstOrDefault(x => x.ApplicationUserId == userId);
        
        if(cart == null)
        {
            cart = new Cart()
            {
                ApplicationUserId = userId,
            };
            _context.Carts.Add(cart);
            _context.SaveChanges();
        }
        return View(cart);
    }
}