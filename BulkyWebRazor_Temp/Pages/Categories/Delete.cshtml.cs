using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
        [BindProperties]
    public class DeleteModel : PageModel
    {

        private readonly ApplicationDbContext _db;

        public Category category { get; set; }

        public DeleteModel(ApplicationDbContext db)
        {
            _db = db;
        }


        public void OnGet(int? Id)
        {
            if (Id != null && Id != 0)
            {
                category = _db.Categories.Find(Id);
            }

        }


        public IActionResult OnPost(int? Id)
        {
            Category? data = category = _db.Categories.Find(Id);

            if (category != null)
            {
                _db.Categories.Remove(data);
                _db.SaveChanges();
                return RedirectToPage("Index");
            }
            return Page();

        }

    }
}
