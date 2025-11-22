using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.Repository
{
    public  class PostPackageRepository : GenericRepository<PostPackage>
    {
        public async Task<IEnumerable<PostPackage>> GetAllPostPackageAsync()
        {
            return await _context.PostPackages
                .Where(p => p.IsActive == true)
                .ToListAsync();
        }
    }
}
