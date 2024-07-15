using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    public class EditModel : PageModel
    {

        private readonly ApplicationDbContext _db;

        [BindProperty]
        public Category category { get; set; }

        public EditModel(ApplicationDbContext db)
        {
            _db = db;
        }


        public void OnGet(int? Id)
        {
            if(Id != null && Id != 0)
            {
            category = _db.Categories.Find(Id);
            }

        }


        public IActionResult OnPost()
        {

            if(category != null)
            {
                _db.Categories.Update(category);
                _db.SaveChanges();
            return RedirectToPage("Index");
            }
            return Page();

        }


    }
}
