using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.ResponseDTO
{
    // response DTO cho cài đặt hệ thống
    public class SystemSettingResponse
    {
        public Guid SettingId { get; set; } // ID của cài đặt
        public string SettingKey { get; set; } = null!; // khóa của cài đặt
        public string SettingValue { get; set; } = null!; // giá trị của cài đặt
        public string? Description { get; set; } // mô tả của cài đặt
        public string? Category { get; set; } // danh mục của cài đặt
        public DateTime? CreatedAt { get; set; } // thời gian tạo
        public DateTime? UpdatedAt { get; set; } // thời gian cập nhật
        public Guid? UpdatedBy { get; set; } // ID người cập nhật
        public string? UpdatedByNavigation { get; set; } // tên người cập nhật
    }
}
