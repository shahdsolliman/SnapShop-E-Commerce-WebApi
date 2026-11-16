using System;
using System.Collections.Generic;

namespace SnapShop.API.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string PictureUrl { get; set; } = null!;
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
    }
}
