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
using Microsoft.IdentityModel.Tokens;

namespace e_store.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private List<string> statuses = new List<string>();
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            statuses = ["processing", "canceled", "delivered", "shipped", "new"];
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
        public async Task<IActionResult> ExportOrdersCsv()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var builder = new System.Text.StringBuilder();
            
            builder.AppendLine("ID,Kupuvac,Datum,Iznos,Status,Produkti_i_Kolicini");

            foreach (var order in orders)
            {
                var buyer = $"{order.ShippingFirstName} {order.ShippingLastName}";
                var date = order.OrderDate.ToString("dd.MM.yyyy");
                
                var productsList = order.OrderDetails.Select(od => $"{od.Product?.Name} (x{od.Quantity})");
                var productsString = string.Join(" | ", productsList);
                
                // builder.AppendLine($"{order.Id},{buyer},{date},{order.TotalAmount},{order.OrderStatus},{productsString}");
                builder.AppendLine($"{order.Id},\"{buyer}\",{date},\"{order.TotalAmount}\",\"{order.OrderStatus}\",\"{productsString}\"");
            }
            
            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "izveshtaj_naracki.csv");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, string? status)
        {
            var order = await _context.Orders.Include(x=>x.OrderDetails).FirstOrDefaultAsync(x => x.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }

            if (status.IsNullOrEmpty())
            {
                return NotFound();
            }
            else if (statuses.Contains(status))
            {
                if (order.OrderStatus == "new" && status != "cancel")
                {
                    var products = order.OrderDetails;
                    foreach (var item in products)
                    {
                        var prod = await _context.Products.FirstOrDefaultAsync(x => x.Id == item.ProductId);
                        if (prod != null)
                        {
                            if (prod.InventoryCount - item.Quantity > 0)
                            {
                                prod.InventoryCount -= item.Quantity;
                            }
                            else
                            {
                                prod.InventoryCount = 0;
                            }
                            
                        }
                    }
                }
                if (status == "cancel")
                {
                    _context.OrderDetails.RemoveRange(order.OrderDetails);
                }
                
                order.OrderStatus = status;
            }

            
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Order");
        }
        
        
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)       
                .ThenInclude(od => od.Product)  
                .FirstOrDefaultAsync(o => o.Id == id);            
            
            if (order == null)
            {
                return NotFound();
            }

            return View(order);

        }
        
        
    }
}
