using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.MenuItemViewModel;
using Tangy.Utility;

namespace Tangy.Controllers
{
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnviroment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemsController(ApplicationDbContext db, IHostingEnvironment hostingEnviroment)
        {
            _db = db;
            _hostingEnviroment = hostingEnviroment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category.ToList(),
                MenuItem = new Models.MenuItem()
            };
        }

        //Get MenuItem
        public async Task<IActionResult> Index()
        {
            var menuItems = _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory);

            return View( await menuItems.ToListAsync());
        }

        //Create Menu
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken ]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
                return View();
            _db.MenuItem.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();
            //Image being saved
            string webRootPath = _hostingEnviroment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemFromDB = _db.MenuItem.Find(MenuItemVM.MenuItem.Id);
            if(files[0]!=null && files[0].Length>0)
            {
                var uplods = Path.Combine(webRootPath, "images");
                var extension = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."), files[0].FileName.Length - files[0].FileName.LastIndexOf("."));
                using (var filestream = new FileStream(Path.Combine(uplods, MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }
                menuItemFromDB.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension;
            }
            else
            {
                var uploads = Path.Combine(webRootPath, @"\images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                menuItemFromDB.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public JsonResult GetSubCategory(int CategoryId)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = (from subcategy in _db.SubCategory
                             where subcategy.CategoryId == CategoryId
                             select subcategy).ToList();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        //Get:Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = _db.SubCategory.Where(p => p.CategoryId == MenuItemVM.MenuItem.CategoryId).ToList();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }
        //Post : Edit Menu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (id!= MenuItemVM.MenuItem.Id)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
                try
                {
                    string webRootPath = _hostingEnviroment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;
                    var menuItemFromDb = _db.MenuItem.Where(m => m.Id == MenuItemVM.MenuItem.Id).FirstOrDefault();
                    if(files[0].Length > 0 && files[0] != null)
                    {
                        //if user uploads
                        var uploads = Path.Combine(webRootPath, "images");
                        var extension_New = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."), files[0].FileName.Length - files[0].FileName.LastIndexOf("."));
                        var extension_Old = menuItemFromDb.Image.Substring(menuItemFromDb.Image.LastIndexOf("."), menuItemFromDb.Image.Length - menuItemFromDb.Image.LastIndexOf("."));
                        if(System.IO.File.Exists(Path.Combine(uploads,MenuItemVM.MenuItem.Id+extension_Old)))
                        {
                            System.IO.File.Delete(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_Old));
                        }
                        using (var filestream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_New), FileMode.Create))
                        {
                            files[0].CopyTo(filestream);
                        }
                        MenuItemVM.MenuItem.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension_New;
                    }
                    if(MenuItemVM.MenuItem.Image != null)
                    {
                        menuItemFromDb.Image = MenuItemVM.MenuItem.Image;
                    }
                    menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
                    menuItemFromDb.Description = MenuItemVM.MenuItem.Name;
                    menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
                    menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
                    menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
                    menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;
                    await _db.SaveChangesAsync();
                }
                catch(Exception ex)
                {

                }
                return RedirectToAction(nameof(Index));
            }
            MenuItemVM.SubCategory = _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToList();
            return View(MenuItemVM);
        }

        //Get:Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //Get:Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //Post Delete : Item
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            string webRootPath = _hostingEnviroment.WebRootPath;
            MenuItem menuItem = await _db.MenuItem.FindAsync(id);
            if(menuItem != null)
            {
                var uploads = Path.Combine(webRootPath, "images");
                var extension = menuItem.Image.Substring(menuItem.Image.LastIndexOf("."), menuItem.Image.Length - menuItem.Image.LastIndexOf("."));
                var imagePAth = Path.Combine(uploads, menuItem.Id + extension);
                if(System.IO.File.Exists(imagePAth))
                {
                    System.IO.File.Delete(imagePAth);
                }
                _db.MenuItem.Remove(menuItem);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}