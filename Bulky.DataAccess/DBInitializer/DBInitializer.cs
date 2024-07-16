using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        //this class is for  automatic creation of database along with admin role

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DBInitializer(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext db)
        {
            
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;

        }

        public void Initialize()
        {

            try
            {

                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }

            }catch (Exception ex)
            {
                

                // Create roles if they are not created
                if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();


                    //if roles are not created, then we will create admin user as well

                    _userManager.CreateAsync(new ApplicationUser
                    {
                        UserName = "admin@abrar.com",
                        Email = "admin@abrar.com",
                        Name = "Abrar Khalifa",
                        PhoneNumber = "6353666071",
                        StreetAddress = "Barkociya Road",
                        State = "Gujarat",
                        City = "Nadiad",
                        PostalCode = "387001"
                    }, "admin@1234").GetAwaiter().GetResult();
                
                
                    ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u=>u.Email == "admin@abrar.com");

                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                
                
                
                }

                return;



            }

        }
    }
}
