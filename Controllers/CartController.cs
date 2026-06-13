using e_store.Data;
using e_store.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace e_store.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private const string CartSessionKey = "Store_Cart_Identifier";
    
    public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<IActionResult> Index()
    {
        var cart = await GetOrCreateCart();
        return View(cart);
    }

    private async Task<Cart> GetOrCreateCart()
    {
        string? sessionId = HttpContext.Session.GetString(CartSessionKey);
        Cart? cart = null;

        if (!string.IsNullOrEmpty(sessionId))
        {
            cart = await _context.Carts.Include(x => x.CartItems).ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        if (cart == null)
        {
            sessionId = Guid.NewGuid().ToString();
            HttpContext.Session.SetString(CartSessionKey, sessionId);

            cart = new Cart()
            {
                SessionId = sessionId,
                CreatedDate = DateTime.Now,
                CartItems = new List<CartItem>() 
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    public async Task<IActionResult> Add(int productId, int quantity)
    {
        var cart = await GetOrCreateCart();

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.CartId == cart.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            var newItem = new CartItem()
            {
                Quantity = quantity,
                CartId = cart.Id,
                ProductId = productId
            };

            _context.CartItems.Add(newItem);
        }

        await _context.SaveChangesAsync();
        
        return RedirectToAction("Details", "Product", new { id = productId });
    }


    [HttpPost]
    public async Task<IActionResult> ClearCart(int id)
    {
        var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(x => x.Id == id);

        if (cart != null)
        {
            _context.CartItems.RemoveRange(cart.CartItems); 
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Cart");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int id, int itemId, string direction)
    {
        var item = await _context.CartItems.FirstOrDefaultAsync(x => x.Id == itemId);
        var cart = await _context.Carts.Include(x => x.CartItems).FirstOrDefaultAsync(x => x.Id == id);
        var cartItems = cart.CartItems.ToList();
        
        if (cart != null && item != null)
        {
            if (direction == "down")
            {
                for (int i = 0; i < cart.CartItems.Count; i++)
                {
                    if (cartItems[i].Id == item.Id)
                    {
                        if (cartItems[i].Quantity > 1)
                        {
                            cartItems[i].Quantity -= 1;
                        } else if (cartItems[i].Quantity == 1)
                        {
                            cartItems[i].Quantity = 0;
                            cart.CartItems.Remove(cartItems[i]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < cart.CartItems.Count; i++)
                {
                    if (cartItems[i].Id == item.Id)
                    {
                        // ovoj pristap ne go pretpocituvam posto moze da vidat lager od sekoj prozivod kolku ima biznisot
                        // var prod = await _context.Products.FirstOrDefaultAsync(x => x.Id == cartItems[i].ProductId);
                        // if (cartItems[i].Quantity + 1 < prod.InventoryCount)
                        // {
                        //     cartItems[i].Quantity += 1;    
                        // }
                        cartItems[i].Quantity += 1;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Cart");
    }
    
    
}