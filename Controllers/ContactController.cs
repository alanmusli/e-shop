using e_store.Data;
using e_store.Models;
using Microsoft.AspNetCore.Mvc;

namespace e_store.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _context;

    public ContactController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public IActionResult Index()
    {
        return View(new ContactFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(ContactFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var newMessage = new ContactMessage()
        {
            CreatedAt = DateTime.Now,
            FullName = model.FullName,
            Email = model.Email,
            Message = model.Message,
            Subject = model.Subject
        };

        try
        {
            _context.ContactMessages.Add(newMessage);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Вашата порака е успешно испратена. Нашиот тим ќе ве контактира наскоро.";
            return RedirectToAction("Index", "Contact");
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Настана грешка при испраќањето на пораката. Ве молиме обидете се подоцна.");
            return View(model);
        }

    }


}