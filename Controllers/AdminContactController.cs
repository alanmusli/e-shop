using e_store.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace e_store.Controllers;

public class AdminContactController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminContactController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var messages = await _context.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
        
        return View(messages);
        
    }


    public async Task<IActionResult> Details(int id)
    {
        var message = _context.ContactMessages.FirstOrDefault(x => x.Id == id);

        if (message == null)
        {
            return NotFound();
        }

        return View(message);
    }
}