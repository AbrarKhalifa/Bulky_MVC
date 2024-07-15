using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BulkyWebRazor_Temp.Models
{
    public class Category
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Category Name")]
        public string? Name { get; set; }

        [DisplayName("Display Order")]


        [Required(ErrorMessage = "Order is required!")]
        [Range(1, 100, ErrorMessage = "Range between 1 to 100")]
        public int DisplayOrder { get; set; }
    
}
}
