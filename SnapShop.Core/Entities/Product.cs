using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }

        public int CategoryId { get; set; } // FK
        public int BrandId { get; set; }  // FK 

        public virtual ProductBrand productBrand { get; set; }  // Navigational Property [ONE]

        public virtual ProductCategory productCategory { get; set; } // Navigational Property [ONE]


    }
}
