using e_store.Data;
using e_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace e_store.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Product
    [HttpGet("Admin/Product/{slug?}/{brandName?}")]
    public async Task<IActionResult> Index(string? slug, string? brandName, string? searchQuery, string? sortOrder)
    {
        var productsQuery = _context.Products
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .AsQueryable();

        ViewBag.CurrentSort = sortOrder;

        switch (sortOrder)
        {
            case "price_asc":
                productsQuery = productsQuery.OrderBy(p => p.Price);
                break;
            case "price_desc":
                productsQuery = productsQuery.OrderByDescending(p => p.Price);
                break;
            case "newest":
            default:
                productsQuery = productsQuery.OrderByDescending(p => p.CreatedDate);
                break;
        }


        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchTerms = searchQuery.Split(' ');

            foreach (var term in searchTerms)
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    continue;
                }

                productsQuery = productsQuery.Where(x =>
                    x.Name.ToLower().Contains(term.ToLower()) ||
                    x.Brand.Name.ToLower().Contains(term.ToLower()));
            }

            ViewBag.SearchQuery = searchQuery;
        }

        if (!string.IsNullOrEmpty(brandName))
        {
            productsQuery = productsQuery.Where(x => x.Brand.Name.ToLower() == brandName.ToLower());
        }

        if (!string.IsNullOrEmpty(slug) && slug != "all")
        {
            var targetCat = await _context.Categories
                .FirstOrDefaultAsync(x => x.Slug == slug);

            if (targetCat != null)
            {
                var validCategs = new List<int>();
                validCategs.Add(targetCat.Id);

                var allCats = await _context.Categories.ToListAsync();
                FindAllDescendantIds(targetCat.Id, allCats, validCategs);

                productsQuery =
                    productsQuery.Where(x => x.CategoryId.HasValue && validCategs.Contains(x.CategoryId.Value));
                ViewBag.CurrentCategoryName = targetCat.Name;
            }
        }
        else
        {
            ViewBag.CurrentCategoryName = "Сите производи";
        }

        return View(await productsQuery.ToListAsync());
    }

    private void FindAllDescendantIds(int parentId, List<Category> allCategories, List<int> resultIds)
    {
        var children = allCategories.Where(c => c.ParentCategoryId == parentId).ToList();

        foreach (var child in children)
        {
            resultIds.Add(child.Id);

            FindAllDescendantIds(child.Id, allCategories, resultIds);
        }
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
            AddCategory(list, child, allCategories, depth + 1);
        }
    }

    // GET: Product/Create
    [HttpGet("Product/Create")]
    public IActionResult Create()
    {
        ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
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

        ViewData["CategoryId"] = new SelectList(dropdownList, "Value", "Text");
        return View();
    }


    [HttpPost("Product/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Id,Name,BrandId,Description,Price,InventoryCount,ImageUrl,CreatedDate,IsActive,SKU,CategoryId")]
        Product product)
    {
        var brand = await _context.Brands.FirstOrDefaultAsync(x => x.Id == product.BrandId);
        var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == product.CategoryId);
        product.SKU = GenerateSKU(product.Name, brand.Name, category.Name);
        if (ModelState.IsValid)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

        return View(product);
    }

    public string GenerateSKU(string? name, string? brand, string? category)
    {
        string cleanCategory = string.IsNullOrWhiteSpace(category) ? "MISC" : category;
        string cleanName = string.IsNullOrWhiteSpace(name) ? "PROD" : name;
        string cleanBrand = string.IsNullOrWhiteSpace(brand) ? "BRND" : brand;

        string catPart = (cleanCategory.Length >= 4 ? cleanCategory.Substring(0, 4) : cleanCategory).ToUpper();
        string prodPart = (cleanName.Length >= 4 ? cleanName.Substring(0, 4) : cleanName).ToUpper();
        string brandPart = (cleanBrand.Length >= 4 ? cleanBrand.Substring(0, 4) : cleanBrand).ToUpper();

        int existingCount = _context.Products.Count();
        int newCount = existingCount + 1;
        
        return $"{catPart}-{prodPart}-{brandPart}--{newCount}";
    }


    // GET: Product/Edit/5
    [HttpGet("Admin/Product/Edit/{id}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

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

        ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
        ViewData["CategoryId"] = new SelectList(dropdownList, "Value", "Text", product.CategoryId);
        return View(product);
    }

    // POST: Product/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Name,BrandId,Description,Price,InventoryCount,ImageUrl,CreatedDate,IsActive,SKU,CategoryId")]
        Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id))
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

        ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Id", product.BrandId);
        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
        return View(product);
    }

    [HttpGet("Admin/Product/Details/{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        var product = await _context.Products.Include(x=>x.Category).Include(x=>x.Brand).FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    


    // GET: Product/Delete/5
    [HttpGet("Admin/Product/Delete/{id}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }

    // POST: Product/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}