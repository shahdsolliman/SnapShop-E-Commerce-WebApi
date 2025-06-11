using SnapShop.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Specifications.Product_Specs;

public class ProductWithBrandAndCategorySpecifications : BaseSpecifications<Product>
{

    // This Constructor Will Be Used For Ceating An Object, That Will Be Used To Get All Products 
    public ProductWithBrandAndCategorySpecifications(ProductSpecParams specParams)
        : base(P =>
                    (string.IsNullOrEmpty(specParams.Search) || P.Name.ToLower().Contains(specParams.Search)) &&
                    (!specParams.brandId.HasValue || P.BrandId == specParams.brandId) &&
                    (!specParams.categoryId.HasValue || P.CategoryId == specParams.categoryId))
    {
        AddIncludes();

        if (!string.IsNullOrEmpty(specParams.sort))
        {
            switch (specParams.sort)
            {
                case "priceAsc":
                    AddOrderBy(P => P.Price);
                    break;

                case "priceDesc":
                    AddOrderByDesc(P => P.Price);
                    break;

                default:
                    AddOrderBy(P => P.Name);
                    break;

            }
        }
        else
            AddOrderBy(P => P.Name);

        // totalProducts = 18 ~ 20 
        // pageSize = 5
        // pageIndex = 3

        ApplyPagination( (specParams.PageIndex - 1) * specParams.PageSize, specParams.PageSize);

    }


    // This Constructor Will Be Used For Ceating An Object, That Will Be Used To Get A Specific Product By His Id
    public ProductWithBrandAndCategorySpecifications(int id) : base(P => P.Id == id)
    {
        AddIncludes();
    }
    private void AddIncludes()
    {
        Includes.Add(x => x.productBrand);
        Includes.Add(x => x.productCategory);
    }

}
