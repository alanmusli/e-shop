using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using e_store.Data;
using e_store.Models;
using e_store.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Hangfire;

namespace e_store.Controllers
{
 
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService; 

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: Order
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var myOrders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(myOrders);
        }



        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Where(c => c.CartItems.Any())
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();
            
            if (cart == null || !cart.CartItems.Any()) 
            {
                return RedirectToAction("Index", "Cart");
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(x => x.Product)
                .Where(c => c.CartItems.Any()) // Игнорирај ги празните кошнички (како ID 21)
                .OrderByDescending(c => c.Id)  // Земи ја последната креирана (твојата моментална)
                .FirstOrDefaultAsync();
            
            
            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }
            
            if (ModelState.IsValid)
            {
                order.UserId = _userManager.GetUserId(User);
                order.OrderDate = DateTime.Now;
                order.TotalAmount = cart.CartItems.Sum(x => x.Quantity * x.Product.Price);

                if (order.OrderDetails == null)
                {
                    order.OrderDetails = new List<OrderDetail>();
                }

                foreach (var item in cart.CartItems)
                {
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    });
                }


                order.OrderStatus = "new";
                
                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();
                
                string emailBody = $"<h3>Успешна нарачка #{order.Id}!</h3><p>Вкупно: {order.TotalAmount} ден.</p>";
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(order.ShippingEmail, "Bistrejd Нарачка", emailBody));
                
                return RedirectToAction("Success", "Order", new { orderId = order.Id });
            }
            
            return View(order);
        }

        
        public IActionResult Success(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}
