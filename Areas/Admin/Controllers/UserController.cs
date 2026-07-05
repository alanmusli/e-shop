using e_store.Areas.Admin.Models.ViewModels;
using e_store.Data;
using e_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace e_store.Areas.Admin.Controllers;

[Area("Admin")] 
[Authorize(Roles = "Admin")]
public class UserController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var usersRoles = new List<UserRoleViewModel>();

        foreach (var user in users)
        {
            var viewModel = new UserRoleViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = await _userManager.GetRolesAsync(user)
            };
            
            usersRoles.Add(viewModel);

        }
        
        return View(usersRoles);
    }

    
    [HttpGet]
    public async Task<IActionResult> ManageRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        var currentRoles = await _userManager.GetRolesAsync(user);
        var model = new UserRoleViewModel()
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = currentRoles,
            AllRoles = allRoles
        };
        

        return View(model);
    }
    
    

    [HttpPost]
    public async Task<IActionResult> ManageRole(string userId, string selectedRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!string.IsNullOrEmpty(selectedRole))
        {
            await _userManager.AddToRoleAsync(user, selectedRole);
        }
        
        return RedirectToAction("Index");
    }


    
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var userOrders = await _context.Orders
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        var currRoles = await _userManager.GetRolesAsync(user);

        var model = new UserDetailViewModel()
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Roles = currRoles,
            Orders = userOrders
        };

        return View(model);


    }
}