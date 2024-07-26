using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class BannerImage
    {

        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public string? Title { get; set; }   

        public string? Description { get; set; }
       
    }
}
