using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.Repository
{
    public class SystemSettingRepository : GenericRepository<Entity.SystemSetting>
    {
        public SystemSettingRepository()
        {
        }

        // lấy giá trị setting theo key
        public async Task<string?> GetSettingValueAsync(string key)
        {
            using (var context = new EvMarketplaceContext())
            {
                var setting = await context.SystemSettings
                    .AsNoTracking() // không theo dõi thay đổi để tăng hiệu suất
                    .FirstOrDefaultAsync(s => s.SettingKey == key); // tìm setting theo key
                return setting?.SettingValue; // trả về giá trị setting hoặc null nếu không tìm thấy 
            }
        }

        // lấy phí hoa hồng hiện tại (%)
        public async Task<decimal> GetCommissionRateAsync()
        {
            var value = await GetSettingValueAsync("COMMISSION_RATE"); // lấy giá trị setting theo key
            if (decimal.TryParse(value, out var rate)) //parse giá trị thành decimal 
            {
                return rate;
            }
            return 2.5m; //nếu không có giá trị hoặc parse thất bại thì trả về mặc định 2.5%
        }

        // cập nhật phí hoa hồng

        public async Task<bool> UpdateCommissionRateAsync(decimal newRate, Guid adminId)
        {
            using (var context = new EvMarketplaceContext())
            {
                var setting = await context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == "COMMISSION_RATE");
                if (setting != null)
                {
                    setting.SettingValue = newRate.ToString();
                    setting.UpdatedAt = DateTime.UtcNow;
                    setting.UpdatedBy = adminId;
                }
                else
                {
                    setting = new SystemSetting
                    {
                        SettingId = Guid.NewGuid(),
                        SettingKey = "COMMISSION_RATE",
                        SettingValue = newRate.ToString("0.##"), // định dạng với tối đa 2 chữ số thập phân
                        Description = "Tỷ lệ phí hoa hồng hệ thống (%)",
                        Category = "PAYMENT", // 
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = adminId // sửa đổi bởi admin có id này
                    };
                    context.SystemSettings.Add(setting);// Thêm setting mới nếu chưa tồn tại
                }
                return await context.SaveChangesAsync() > 0; // Lưu thay đổi vào database
            }
        }

        // lấy tất cả setting theo category
        public async Task<List<SystemSetting>> GetSettingsByCategoryAsync(string category)
        {
            using (var context = new EvMarketplaceContext())
            {
                return await context.SystemSettings
                    .Include(s => s.UpdatedByNavigation) // bao gồm thông tin người cập nhật
                    .Where(s => s.Category == category) // lọc theo category
                    .OrderBy(s => s.SettingKey) // sắp xếp theo key
                    .AsNoTracking() // không theo dõi thay đổi để tăng hiệu suất
                    .ToListAsync(); // trả về danh sách setting
            }
        }

        //lấy setting kèm thông tin người cập nhật

        public async Task<SystemSetting?> GetSettingWithUserAsync(string key)
        {
            using (var context = new EvMarketplaceContext())
            {
                return await context.SystemSettings
                    .Include(s => s.UpdatedByNavigation) // bao gồm thông tin người cập nhật
                    .AsNoTracking() // không theo dõi thay đổi để tăng hiệu suất
                    .FirstOrDefaultAsync(s => s.SettingKey == key); // trả về setting theo key
            }
        }
    }
}


