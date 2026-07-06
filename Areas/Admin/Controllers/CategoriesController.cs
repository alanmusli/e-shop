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

namespace e_store.Areas.Admin.Controllers

{
    [Area("Admin")] 
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


        // GET: Categories/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var categories = _context.Categories.ToList();
            var rootCategories = new List<Category>();
            
            foreach (var category in categories)
            {
                if (category.ParentCategoryId == null)
                {
                    rootCategories.Add(category);
                }
            }

            var dropdownList = new List<SelectListItem>();
            
            foreach (var cat in rootCategories)
            {
                AddCategory(dropdownList, cat, categories, 0);
            }
            

            ViewData["ParentCategoryId"] = new SelectList(dropdownList, "Value", "Text");            
            
            return View();
        }


        private void AddCategory(List<SelectListItem> list, Category current, List<Category> allCategories, int depth)
        {
            
            string spacing = new string('-', depth * 2);
            string displayText = depth > 0 ? $"{spacing} {current.Name}" : current.Name;
            
            list.Add(new SelectListItem()
            {
                Value = current.Id.ToString(),
                Text = displayText
            });

            var children = allCategories.Where(x => x.ParentCategoryId == current.Id).ToList();

            foreach (var child in children)
            {
                AddCategory(list, child, allCategories, depth+1);
            }
        }
        
        private void AddCategory(List<SelectListItem> list, Category current, List<Category> allCategories, int depth, int currentEditingId)
        {

            if (current.Id == currentEditingId)
            {
                return;
            }
            
            string spacing = new string('-', depth * 2);
            string displayText = depth > 0 ? $"{spacing} {current.Name}" : current.Name;
            
            list.Add(new SelectListItem()
            {
                Value = current.Id.ToString(),
                Text = displayText
            });

            var children = allCategories.Where(x => x.ParentCategoryId == current.Id).ToList();

            foreach (var child in children)
            {
                AddCategory(list, child, allCategories, depth+1, currentEditingId);
            }
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Slug,ParentCategoryId")] Category category)
        {
            var currentSlug = category.Name.ToLower().Trim().Replace(" ", "-");
            var slugParts = new List<string> { currentSlug };
            if (category.ParentCategoryId == -1)
            {
                category.ParentCategoryId = null;

            }
            else {
            var currentParentId = category.ParentCategoryId;
            slugParts = SlugCreator(currentParentId, category);
            }

            category.Slug = string.Join("/", slugParts);
            
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }


        private List<string> SlugCreator(int? currentParentId, Category category)
        {
            var currentSlug = category.Name.ToLower().Trim().Replace(" ", "-");
            var slugParts = new List<string> { currentSlug };
            
            while (currentParentId != null)
            {
                var parentNode = _context.Categories.FirstOrDefault(x => x.Id == currentParentId);
                if (parentNode != null)
                {
                    var parentSlugPart = parentNode.Name.ToLower().Trim().Replace(" ", "-");
                    slugParts.Add(parentSlugPart);
                    currentParentId = parentNode.ParentCategoryId;
                }
                else
                {
                    break;
                }
            }

            slugParts.Reverse();

            return slugParts;

        }


        // GET: Categories/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            
            var allCategories = await _context.Categories.ToListAsync();
            
            var rootCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();
            var dropdownList = new List<SelectListItem>();
            
            foreach (var root in rootCategories)
            {
                AddCategory(dropdownList, root, allCategories, 0, category.Id);
            }
            
            ViewData["ParentCategoryId"] = new SelectList(dropdownList, "Value", "Text", category.ParentCategoryId);
            
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,Description,Slug,ParentCategoryId")] Category category)
        {
            var oldCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            var oldSlug = oldCategory?.Slug;
            
            if (id != category.Id)
            {
                return NotFound();
            }
            
            
            var newSlug = "";
            
            if (category.ParentCategoryId == -1)
            {
                category.ParentCategoryId = null;
                newSlug = "/" + category.Name.ToLower().Trim().Replace(" ", "-");
            }
            else
            {
                var parentCategory = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == category.ParentCategoryId);
                if (parentCategory != null)
                {
                    var cleanName = category.Name.ToLower().Trim().Replace(" ", "-");
                    newSlug = parentCategory.Slug.TrimEnd('/') + "/" + cleanName;
                }
            }
            
            category.Slug = newSlug;

            if (ModelState.IsValid)
            {
                try
                {
                    
                    
                    if (oldSlug != newSlug)
                    {
                        var categories = await _context.Categories.AsNoTracking().ToListAsync();
                        UpdateChildSlug(category.Id, newSlug, categories); 
                    }
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        private void UpdateChildSlug(int parentId, string parentNewSlug, List<Category> allCategories)
        {
            var children = allCategories.Where(c => c.ParentCategoryId == parentId).ToList();

            foreach (var child in children)
            {
                var cleanName = child.Name.ToLower().Trim().Replace(" ", "-");
                
                child.Slug = parentNewSlug.TrimEnd('/') + "/" + cleanName;
                
                _context.Update(child);
                
                UpdateChildSlug(child.Id, child.Slug, allCategories);
            }
        }
            
        // GET: Categories/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Categories/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}