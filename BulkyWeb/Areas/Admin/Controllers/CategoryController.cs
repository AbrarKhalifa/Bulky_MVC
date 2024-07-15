using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {

            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Category> list = _unitOfWork.Category.GetAll().ToList();
            return View(list);
        }


        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {

            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display Order cannot exactly match the Name.");
            }
            //if (category.Name != null && category.Name.ToLower() == "test")
            //{
            //    ModelState.AddModelError("", "Test is invalid value.");
            //}

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "Category Created Successfully.";
                return RedirectToAction("Index");

            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {

                return NotFound();

            }
            Category? data = _unitOfWork.Category.Get(c => c.Id == id);
            return View(data);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {

                ModelState.AddModelError("Name", "Category Name is not exactly same as a Category Order.");

            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
                TempData["success"] = "Category Updated Successfully.";

                return RedirectToAction("Index");

            }

            return View();

        }


        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {

                return NotFound();

            }
            Category? data = _unitOfWork.Category.Get(c => c.Id == id);
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? Id)
        {
            if (Id == null || Id == 0)
            {

                return NotFound();

            }

            Category cat = _unitOfWork.Category.Get(c => c.Id == Id);

            if (cat != null)
            {
                _unitOfWork.Category.Remove(cat);
                _unitOfWork.Save();
                TempData["success"] = "Category Deleted Successfully.";

                return RedirectToAction("Index");

            }

            return View("Delete");


        }




    }
}
