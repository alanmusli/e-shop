using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using e_store.Data;
using e_store.Models;

namespace e_store.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        [HttpGet("Product")] 
        [HttpGet(@"Product/{slug:regex(^(?!Details$).*$)?}/{brandName?}")]        
        public async Task<IActionResult> Index(string? slug, string? brandName, string? searchQuery, string? sortOrder, int pg = 1)
        {
            const int pageSize = 10;
            if (pg < 1) pg = 1;
            
            
            var productsQuery = _context.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .AsQueryable();
            
            
            
            ViewBag.CurrentSort = sortOrder;
            ViewBag.SearchQuery = searchQuery; 
            ViewBag.CurrentSlug = slug;
            ViewBag.CurrentBrand = brandName;
            
            
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

            if (slug != "all")
            {
                var targetCat = await _context.Categories
                    .FirstOrDefaultAsync(x => x.Slug == slug);

                if (targetCat != null)
                {
                    var validCategs = new List<int>();
                    validCategs.Add(targetCat.Id);

                    var allCats = await _context.Categories.ToListAsync();
                    FindAllDescendantIds(targetCat.Id, allCats, validCategs);

                    productsQuery = productsQuery.Where(x => x.CategoryId.HasValue && validCategs.Contains(x.CategoryId.Value));
                    ViewBag.CurrentCategoryName = targetCat.Name;
                }
            }
            else
            {
                ViewBag.CurrentCategoryName = "Сите производи";
            }
            
            int totalProducts = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((decimal)totalProducts / pageSize);
            
            ViewBag.CurrentPage = pg;
            ViewBag.TotalPages = totalPages;
            
            var products = await productsQuery
                .Skip((pg - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return View(products);
            
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
        

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
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
     
    }
}
