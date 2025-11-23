using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.RequestDTO
{
    public class PostPackgeDTO
    {
    }

    public class CreatePostPackageDTO
    {
        public string PackageName { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }

    }
    public class UpdatePostPackageDTO
    {
        public Guid Id { get; set; }
        public string PackageName { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
    }
    public class PostPackgeDTOResponse
    {
        public Guid Id { get; set; }
        public string PackageName { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        
        public DateTime? CreatedAt { get; set; }

        public bool? isActive { get; set; }    
    }
}
