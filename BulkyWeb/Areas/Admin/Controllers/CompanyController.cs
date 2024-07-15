using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.CodeDom;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {

            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            List<Company> list = _unitOfWork.Company.GetAll().ToList();

            return View(list);
        }

        [HttpGet]
        public IActionResult Upsert(int? id) {
        
            if(id != null)
            {
               var getData =  _unitOfWork.Company.Get(x => x.Id == id);
                return View(getData);
            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        public IActionResult Upsert(Company company) {

            if(company == null) { return NotFound(); }
            if(company.Id == 0 || company.Id == null)
            {
                _unitOfWork.Company.Add(company);
                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";

                return RedirectToAction("Index");
                
            }
            else
            {
                _unitOfWork.Company.Update(company);
                _unitOfWork.Save();
                TempData["success"] = "Company updated successfully";

                return RedirectToAction("Index");
            }
        }

        [HttpDelete]
        public IActionResult Delete(int? id) {

            var companyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error While deleting..." });
            }

            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Company Deleted Successfully." });
        }


        // Region API CALL for searching index data

        //1. add css and js file in _Layout file for data table

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> list = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = list });
        }

        //2. product.js file
        //3. add js file and dataTable Id in index page


        //end region
    }
}
