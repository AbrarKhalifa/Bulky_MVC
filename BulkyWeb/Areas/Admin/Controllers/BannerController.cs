using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using BulkyWeb.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;
using Stripe.Climate;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
 

        public BannerController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }


        // GET: BannerController
        public ActionResult Index()
        {
            BannerVM banner = new BannerVM()
            {
                BannerImage = _unitOfWork.BannerImage.GetAll().ToList()
            };
            return View(banner);
        }

        [HttpGet]
        public IActionResult Create(int? id)
        {
            BannerVM bannerVM = new()
            {

                Banner = new BannerImage()
            };

            if (id == null || id == 0)
            {
                // Create   
                return View();
            }
            else
            {
                // Update
                bannerVM.Banner = _unitOfWork.BannerImage.Get(u => u.Id == id);
                return View(bannerVM);
            }

        }

        [HttpPost]
        public IActionResult Create(BannerVM? bannerVM, IFormFile file)
        {
            if (bannerVM != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    string bannerPath = Path.Combine(wwwRootPath, @"images\banner");


                    if (!string.IsNullOrEmpty(bannerVM.Banner.ImageUrl))
                    {
                        // delete old image
                        var oldImagePath = Path.Combine(wwwRootPath, bannerVM.Banner.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                        using (var fileStream = new FileStream(Path.Combine(bannerPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        bannerVM.Banner.ImageUrl = @"images\banner\" + fileName;

                    



                    if (bannerVM.Banner.Id == 0)
                    {
                        _unitOfWork.BannerImage.Add(bannerVM.Banner);
                    }
                    else
                    {
                        _unitOfWork.BannerImage.Update(bannerVM.Banner);
                    }

                    _unitOfWork.Save();
                    TempData["success"] = "Banner Image created/updated successfully";
                    return RedirectToAction("Index");

                }


            }
            else
            {
                bannerVM.Banner = _unitOfWork.BannerImage.Get(x => x.Id == bannerVM.Banner.Id);
            }
                return View(bannerVM);

        }

    }
}
