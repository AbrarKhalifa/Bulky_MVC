using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        private BannerVM BannerVM { get; set; }


        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        [HttpGet]
        public IActionResult Index(string? search, string? status, string? categories)
        {

            IEnumerable<Product> productList;

            var category = _unitOfWork.Category.GetAll().ToList();
            ViewBag.CategoryList = new SelectList(category, "Id", "Name");

            if (search != null)
            {
                productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages").Where(x => x.Title.ToLower().StartsWith(search)).ToList();

                if (productList.Count() == 0)
                {
                    productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages").ToList();
                    TempData["error"] = "No result found!";
                }

               
            }
            else
            { 
             productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages").ToList();

              
            }


            switch (status)
            {
                case "PriceASC":
                    productList = productList.OrderBy(x => x.Price100);
                    break;
                case "PriceDESC":
                    productList = productList.OrderByDescending(x => x.Price100);
                    break;
                default:
                    break;

            }

            if(categories != null)
            {
                productList = productList.Where(x => x.Category.Name == categories);
                if(productList.Count() == 0)
                {
                    TempData["error"] = "No Book Found!";
                }
            }


            BannerVM = new BannerVM()
            {
                Product = productList,
                BannerImage = _unitOfWork.BannerImage.GetAll().ToList()
            };

            return View(BannerVM);

        }



        [HttpGet]
        public IActionResult Details(int id)
        {
            ShoppingCart shoppingCart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category,ProductImages"),
                Count = 1,
                ProductId = id
            };
            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {

            if (shoppingCart.Id != 0 || shoppingCart.Id != null)
            {
                shoppingCart.Id = 0;
            }

            // through this we can get logged in user id

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            // end

            var getCartFromDb = _unitOfWork.ShoppingCart.Get(x => x.ApplicationUserId == userId && x.ProductId == shoppingCart.ProductId);

            if (getCartFromDb != null)
            {
                // update card
                getCartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(getCartFromDb);
                _unitOfWork.Save();



            }
            else
            {
                // insert new card

                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId).Count());

            }


            TempData["success"] = "Cart updated successfully.";

            return RedirectToAction("Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
