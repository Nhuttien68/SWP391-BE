using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.Repository
{
    public class BatteryBrandRepository : GenericRepository<BatteryBrand>
    {
        public async Task<IEnumerable<BatteryBrand>> GetAllBatteryBrandAsync()
        {
            return await _context.BatteryBrands
                .Where(p => p.Status == "Active")
                .ToListAsync();
        }
    }
}
