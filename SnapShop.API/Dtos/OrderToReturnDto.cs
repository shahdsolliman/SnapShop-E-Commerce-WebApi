using SnapShop.Core.Entities.OrderAggregate;

namespace SnapShop.API.Dtos
{
    public class OrderToReturnDto
    {
        public int Id { get; set; }
        public string BuyerEmail { get; set; } = null!;
        public DateTimeOffset Date { get; set; }
        public string Status { get; set; } = null!;
        public Address ShippingAddress { get; set; } = null!;
        public string DeliveryMethod { get; set; } = null!;
        public decimal DeliveryMethodCost { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Total { get; set; }
        public ICollection<OrderItemDto> Items { get; set; } = new HashSet<OrderItemDto>();
        public string PaymentIntenId { get; set; } = string.Empty;
    }
}
