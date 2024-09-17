using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Data; // Για το ApplicationDbContext


namespace Bulky.DataAccess.Data
{ 
public class CategoryController : Controller

{
    private readonly ApplicationDbContext _db;

    public CategoryController(ApplicationDbContext db)

    {
        _db = db;

    }
    public IActionResult Index()
    {
        List<Category> ObjectCategoryList = _db.Categories.ToList();
        return View(ObjectCategoryList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Category obj)
    {
        
        if (obj.Name == obj.DisplayOrder.ToString())
        {
         ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name.");
        }
      
            if (ModelState.IsValid)
        {
            _db.Categories.Add(obj);
            _db.SaveChanges();
            TempData["success"] = "Category created successfully";
            return RedirectToAction("Index");
        }
        return View();
    }
    public IActionResult Edit(int? id)
    {   if (id== null || id == 0 ){
            return NotFound();
        }
        Category categoryFromDb = _db.Categories.Find(id);
        if (categoryFromDb == null)
        {
            return NotFound();
        }

        return View(categoryFromDb);
    }

    [HttpPost]
    public IActionResult Edit(Category obj)
    {

        // if (obj.Name == obj.DisplayOrder.ToString())
        // {
        //      ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name.");
       // }

        if (ModelState.IsValid)
        {
            _db.Categories.Update(obj);
            _db.SaveChanges();
            TempData["success"] = "Category update successfully";
            return RedirectToAction("Index");
        }
        return View();
    }

    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        Category categoryFromDb = _db.Categories.Find(id);
        if (categoryFromDb == null)
        {
            return NotFound();
        }

        return View(categoryFromDb);
    }

    [HttpPost,ActionName("Delete")]
    public IActionResult DeletePOST(int? id)
    {
        Category? obj = _db.Categories.Find(id);
        if (obj == null)
        {
            return NotFound();
        }
        _db.Categories.Remove(obj);
        _db.SaveChanges();
        TempData["success"] = "Category delete successfully";
        return RedirectToAction("Index");
        // if (obj.Name == obj.DisplayOrder.ToString())
        // {
        //      ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name.");
        // }

        
    }
}

}