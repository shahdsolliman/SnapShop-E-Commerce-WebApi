using System.ComponentModel.DataAnnotations;

namespace SnapShop.API.Dtos
{
    public class OrderDto
    {
        [Required]
        public string BasketId { get; set; } = null!;

        [Required]
        public int DeliveryMethodId { get; set; }

        [Required]
        public AddressDto ShippingAddress { get; set; } = null!;

        public string? BuyerEmail { get; set; }

        public string? Phone { get; set; }
    }
}
