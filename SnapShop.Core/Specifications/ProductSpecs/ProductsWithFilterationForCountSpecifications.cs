using SnapShop.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Specifications.ProductSpecs
{
    public class ProductsWithFilterationForCountSpecifications : BaseSpecifications<Product>
    {
        public ProductsWithFilterationForCountSpecifications(ProductSpecParams specParams) 
            :base ( P =>
                        (string.IsNullOrEmpty(specParams.Search) || P.Name.ToLower().Contains(specParams.Search)) &&
                        (!specParams.brandId.HasValue || P.BrandId == specParams.brandId) &&
                        (!specParams.categoryId.HasValue || P.CategoryId == specParams.categoryId)
            )
        {
            
        }
    }
}
