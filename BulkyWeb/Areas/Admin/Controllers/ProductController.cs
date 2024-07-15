using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace BulkyWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {


        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {

            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Product> list = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();

            return View(list);
        }


        [HttpGet]
        public IActionResult Upsert(int? id)
        {

        
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
            Product = new Product()
            };

            if (id == null ||  id == 0)
            {
                // Create   
                return View(productVM);
            }
            else
            {
                // Update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }


        }


        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {

            if (obj.Product.Title == obj.Product.Author.ToString())
            {
                ModelState.AddModelError("Title", "Title cannot exactly match the author name.");
            }
           
            if (ModelState.IsValid)
            {

                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"Images\product");

                    if(!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        // delete old image at the time of updating if user want to update that image
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));

                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }


                    }


                    using(var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\Images\product\" + fileName;
                }

                if(obj.Product.Id == 0)
                {

                    _unitOfWork.Product.Add(obj.Product);
                    _unitOfWork.Save();

                    TempData["success"] = "Product Created Successfully.";

                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                    _unitOfWork.Save();
                    TempData["success"] = "Product Updated Successfully.";

                }   

                return RedirectToAction("Index");

            }
            else
            {
                obj.CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(obj.CategoryList);  
            }
        }


       


        // Region API CALL for searching index data

        //1. add css and js file in _Layout file for data table

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> list = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = list });
        }

        //2. product.js file
        //3. add js file and dataTable Id in index page


        //end region

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error While deleting..." });
            }

            if (productToBeDeleted.ImageUrl != null)
            {

                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));


                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Product Deleted Successfully." });

        }

        //#endregion



    }
}
