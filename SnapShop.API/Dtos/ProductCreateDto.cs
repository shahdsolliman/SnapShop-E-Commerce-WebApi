using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SnapShop.API.Dtos
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "اسم المنتج أو البضاعة مطلوب")]
        [MaxLength(150, ErrorMessage = "اسم المنتج لا يزيد عن 150 حرف")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, 1000000, ErrorMessage = "السعر يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal? Price { get; set; }

        public int? CategoryId { get; set; }

        public int? BrandId { get; set; }

        public IFormFile? Image { get; set; }
    }
}
