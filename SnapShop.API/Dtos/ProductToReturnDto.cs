using SnapShop.Core.Entities;

namespace SnapShop.API.Dtos
{
    public class ProductToReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string Description { get; set; } = null!;
        public string PictureUrl { get; set; } = null!;

        public int CategoryId { get; set; }
        public int BrandId { get; set; }  

        public string productBrand { get; set; } = null!;  

        public string productCategory { get; set; } = null!; 


    }
}
