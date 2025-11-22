using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.Repository
{
    public class VehilceBrandRepository : GenericRepository<VehicleBrand>
    {
        public async Task<IEnumerable<VehicleBrand>> GetAllVehicleBrandAsync()
        {
            return await _context.VehicleBrands
                .Where(p => p.Status == "Active")
                .ToListAsync();
        }
    }
}
