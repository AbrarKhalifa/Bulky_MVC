using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.CodeDom;
using System.Collections.Generic;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;


        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        

        public IActionResult Index()
        {
            
            return View();
        }

       
        [HttpDelete]
        public IActionResult Delete(int? id) {

           

            return Json(new { success = true, message = "Company Deleted Successfully." });
        }


        // Region API CALL for searching index data

        //1. add css and js file in _Layout file for data table

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> list = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();
    
            foreach (var user in list)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

                if(user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }

            }

            return Json(new { data = list });
        }

        //2. product.js file
        //3. add js file and dataTable Id in index page


        //end region

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objUser = _unitOfWork.ApplicationUser.Get(x => x.Id == id);
            if (objUser == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if(objUser.LockoutEnd != null && objUser.LockoutEnd > DateTime.Now)
            {
                // user is currently locked we need to unlock them
                objUser.LockoutEnd = DateTime.Now;
            }
            else
            {
                objUser.LockoutEnd = DateTime.Now.AddYears(1000);

            }

            _unitOfWork.ApplicationUser.Update(objUser);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation successfull" });
        }

        [HttpGet]
        public IActionResult RoleManagment(string userId)
        {
            RoleManagementVM RoleVM = new()
           {
               ApplicationUser =_unitOfWork.ApplicationUser.Get(u=>u.Id==userId,includeProperties:"Company"),
                RoleList  = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(x => x.Id == userId))
                .GetAwaiter().GetResult().FirstOrDefault();

            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagementVM roleManagementVM)
        {

            var oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(x => x.Id == roleManagementVM.ApplicationUser.Id))
                .GetAwaiter().GetResult().FirstOrDefault();

                ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);

            if (!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                // a role was updated

                if(roleManagementVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }

                if ( oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }


                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                TempData["success"] = "User Role Updated Successfully.";

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagementVM.ApplicationUser.CompanyId) {

                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            return RedirectToAction("Index");
        }
    }
}
