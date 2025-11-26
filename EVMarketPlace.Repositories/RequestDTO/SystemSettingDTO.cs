using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.RequestDTO
{
    //request DTO để cập nhật tỷ lệ hoa hồng
    public class UpdateCommissionRateRequest
    {
        [Required(ErrorMessage = "Tỷ lệ hoa hồng không được để trống")]
        [Range(0, 100, ErrorMessage = "Tỷ lệ hoa hồng phải từ 0% đến 100%")]
        public decimal CommissionRate { get; set; } // tỷ lệ hoa hồng mới 
    }

}
