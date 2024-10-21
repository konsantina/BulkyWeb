using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using BulkyBook.DataAccess.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using BulkyBook.DataAccess.Repository;
using System.Collections.Generic;
using BulkyBook.Models.ViewModels;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;



namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
     
        public CompanyController(IUnitOfWork initOfWork)
        {
            _unitOfWork = initOfWork;
          
        }

        public IActionResult Index()

        {
            List<Company> ObjectCompanyList = _unitOfWork.Company.GetAll().ToList();
            return View(ObjectCompanyList);
        }

        public IActionResult Upsert(int? id)
        {
          
            
            if (id == null || id == 0)
            {
                //Create

                return View( new Company());
            }
            else
            {
                //Update
                Company companyObj= _unitOfWork.Company.Get(u => u.Id == id);
                return View(companyObj);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {

            if (ModelState.IsValid)
            {
              
               
                if (companyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(companyObj);

                }
                else
                { 
                _unitOfWork.Company.Update(companyObj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else 
            {
              
                return View(companyObj);
            }
        }
        
     
       

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()      
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
             var CompanyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Company.Remove(CompanyToBeDeleted);
            _unitOfWork.Save();


           //List<Company> objCompanyList = _unitOfWork.Company.GetAll(includeProperties: "Category").ToList();
            return Json(new { success= true, message = "Delete Successful" });
        }

        #endregion
    }
}
