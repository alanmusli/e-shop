using e_store.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e_store.Areas.Admin.Controllers
{
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersApiController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders.OrderByDescending(x => x.OrderDate).ToListAsync();


            var tableData = orders.Select(o => new
            {
                id = o.Id,
                date = o.OrderDate.ToString("dd MMM yyyy"),
                time = o.OrderDate.ToString("HH:mm"),
                customerName = $"{o.ShippingFirstName} {o.ShippingLastName}",
                phone = o.ShippingPhoneNumber,
                amount = o.TotalAmount.ToString("N0") + " ден.",
                statusLower = o.OrderStatus
            });

            return Ok(tableData);
        }
    }
}
