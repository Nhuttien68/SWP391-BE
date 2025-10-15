using EVMarketPlace.Repositories.RequestDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.RequestDTO
{
    public class BatteryBrandRequestDTO
    {
        [Required(ErrorMessage = "Brand Name not allow null")]
         public string BrandName { get; set; }
    }
    public class UpdateBatteryBrandRequestDTO
    {
        [Required(ErrorMessage = "BatteryBrandId not allow null")]
       public Guid BatteryBrandId { get; set; }
        [Required(ErrorMessage = "BrandName not allow null")]
       public string BrandName { get; set; }
    }
}
