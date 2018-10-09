using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.SubCategoryViewModel;

namespace Tangy.Controllers
{
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public string StatusMessage { get; set; }
        public SubCategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        //Get ACtion
        public async Task<IActionResult> Index()
        {
            var subCategory = _db.SubCategory.Include(s => s.CategoryPlus);
            return View(await subCategory.ToListAsync());
        }

        //Get Action for create
        public  IActionResult Create()
        {
            SubCategoryandCategoryViewModel model = new SubCategoryandCategoryViewModel()
            {
                CategoryList =_db.Category.ToList(),
                SubCategory = new SubCategory(),
                SubCategoryList = _db.SubCategory.OrderBy(p=>p.Name).Select(p=>p.Name).Distinct().ToList()
            };
            return View(model);
        }

        //Post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryandCategoryViewModel model)
        {
            if(ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Where(s => s.Name == model.SubCategory.Name).Count();
                var doesSubCategoryAndCatExists = _db.SubCategory.Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId).Count();

                if (doesSubCategoryExists > 0 && model.isNew)
                {
                    //error
                    StatusMessage = "Error: SubCategory already Exists";
                }
                else
                {
                    if(doesSubCategoryExists == 0 && !model.isNew)
                    {
                        //error
                        StatusMessage = "Error: SubCategory didn't Exists";
                    }
                    else
                    {
                        if(doesSubCategoryAndCatExists > 0)
                        {
                            //error
                            StatusMessage = "Error: SubCategory  and Category  combination already Exists";
                        }
                        else
                        {
                            _db.Add(model.SubCategory);
                            await _db.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }
            SubCategoryandCategoryViewModel modelVm = new SubCategoryandCategoryViewModel()
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = new SubCategory(),
                SubCategoryList = _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToList(),
                StatusMessage = StatusMessage 
            };
            return View(modelVm);
        }

        //Get Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            var subCategory =  await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            if(subCategory==null)
            {
                return NotFound();
            }
            SubCategoryandCategoryViewModel model = new SubCategoryandCategoryViewModel()
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = subCategory,
                SubCategoryList = _db.SubCategory.Select(p => p.Name).Distinct().ToList()
            };
            return View(model);
        }

        //Post Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Edit(int id, SubCategoryandCategoryViewModel model)
        {
            if(ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Where(s => s.Name == model.SubCategory.Name).Count();
                var doesSubCategoryAndCatExists = _db.SubCategory.Where(s => s.Name == model.SubCategory.Name && s.CategoryId == model.SubCategory.CategoryId).Count();
                if(doesSubCategoryExists ==0)
                {
                    StatusMessage = "Error : SubCategory does not exists. You cannot add a new subcategory here";
                }
                else
                {
                    if(doesSubCategoryAndCatExists > 0)
                    {
                        StatusMessage = "Error : Category and SubCategory combination alreay exists. ";
                    }
                    else
                    {
                        var subCategoryfromDB = _db.SubCategory.Find(id);
                        subCategoryfromDB.Name = model.SubCategory.Name;
                        subCategoryfromDB.CategoryId = model.SubCategory.CategoryId;
                        await _db.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            SubCategoryandCategoryViewModel modelVM = new SubCategoryandCategoryViewModel()
            {
                CategoryList = _db.Category.ToList(),
                SubCategory = model.SubCategory,
                SubCategoryList = _db.SubCategory.Select(p => p.Name).Distinct().ToList(),
                StatusMessage = StatusMessage
            };
            return View(modelVM);

        }

        //Get Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.Include(s => s.CategoryPlus).SingleOrDefaultAsync(m => m.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }
            return View(subCategory);
        }

        //Get Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.Include(s => s.CategoryPlus).SingleOrDefaultAsync(m => m.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }
            return View(subCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            _db.SubCategory.Remove(subCategory);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}