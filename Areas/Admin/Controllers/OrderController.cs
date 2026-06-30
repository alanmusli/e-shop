using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using e_store.Data;
using e_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace e_store.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet] 
        public async Task<IActionResult> Delivery(int? id)
        {
            var item = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            item.OrderStatus = "delivered";
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Order");
            
        }
        
        
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)       
                .ThenInclude(od => od.Product)  
                .FirstOrDefaultAsync(o => o.Id == id);            if (order == null)
            {
                return NotFound();
            }

            return View(order);

        }


        
    }
}
