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

namespace e_store.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            // var applicationDbContext = _context.Categories.Include(c => c.SubCategories).Include(c=>c.ParentCategory).Where(c=>c.ParentCategoryId == null );
            // return View(await applicationDbContext.ToListAsync());
            var allCategories = await _context.Categories.ToListAsync();
            var lookup = allCategories.ToLookup(c => c.ParentCategoryId);
            ViewData["CategoryLookup"] = lookup;

            var rootCategories = lookup[null].ToList();
            return View(rootCategories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
}