using System.ComponentModel.DataAnnotations;

namespace SnapShop.API.Dtos
{
    public class BasketItemDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage ="Price Must Be Greater Than ZERO!!")]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage ="Quantity Must Be One At Least!!")]
        public int Quantity { get; set; }

        [Required]
        public string PictureUrl { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Brand { get; set; }
    }
}