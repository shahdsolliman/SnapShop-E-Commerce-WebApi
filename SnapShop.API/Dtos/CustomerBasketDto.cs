using SnapShop.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace SnapShop.API.Dtos
{
    public class CustomerBasketDto
    {
        [Required]
        public string Id { get; set; } = null!;
        public string? PaymentInentId { get; set; }
        public string? ClientSecret { get; set; }
        public int? DelieveryMethod { get; set; }
        public decimal ShippingPrice { get; set; }
        public List<BasketItemDto> Items { get; set; } = new List<BasketItemDto>();
    }
}
