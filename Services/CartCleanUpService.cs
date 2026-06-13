using System.Linq.Expressions;
using e_store.Data;
using Microsoft.EntityFrameworkCore;

namespace e_store.Services;

public class CartCleanUpService : BackgroundService
{
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CartCleanUpService> _logger;


    public CartCleanUpService(IServiceProvider serviceProvider, ILogger<CartCleanUpService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cart cleanup starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var expirationDate = DateTime.UtcNow.AddDays(-7);
                    // var expirationDate = DateTime.Now.AddMinutes(-1); //za testiranje

                    var oldCarts = await context.Carts.Include(x=>x.CartItems).Where(x => x.CreatedDate < expirationDate)
                        .ToListAsync(stoppingToken);

                    if (oldCarts.Any())
                    {
                        int cartItemsCount = 0;
                        for (int i = 0; i < oldCarts.Count; i++)
                        {
                            cartItemsCount += oldCarts[i].CartItems.Count;
                            context.CartItems.RemoveRange(oldCarts[i].CartItems);
                            context.Carts.Remove(oldCarts[i]);
                        }
                        // context.Carts.RemoveRange(oldCarts);
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"Successfully purged {oldCarts.Count} abandoned staging carts and cartItems: {cartItemsCount}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A error occured while attempting to remove old carts");    
            }
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            //za testiranje 
            // await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}