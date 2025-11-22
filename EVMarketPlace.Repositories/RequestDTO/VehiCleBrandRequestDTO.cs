using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.RequestDTO
{
    public class VehiCleBrandRequestDTO
    {
        [Required(ErrorMessage ="Brand Name not allow null")]
         public string BrandName { get; set; }
       
    }
    public class VehiCleBrandUpdateRequestDTO
    {
        [Required(ErrorMessage = "Brand Id not allow null")]
        public Guid BrandId { get; set; }
        [Required(ErrorMessage = "Brand Name not allow null")]
        public string BrandName { get; set; }
    }
}
