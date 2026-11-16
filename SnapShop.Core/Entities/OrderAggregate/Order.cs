using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Entities.OrderAggregate
{
    public class Order : BaseEntity
    {
        public Order()
        {
            
        }
        public Order(string buyerEmail, Address shippingAddress, decimal subTotal, DeliveryMethod deliveryMethod, ICollection<OrderItem> items, string paymentIntentId)
        {
            BuyerEmail = buyerEmail;
            ShippingAddress = shippingAddress;
            SubTotal = subTotal;
            DeliveryMethod = deliveryMethod;
            Items = items;
            PaymentIntenId = paymentIntentId ?? string.Empty;
            Date = DateTimeOffset.UtcNow;
        }

        public string BuyerEmail { get; set; } = string.Empty;
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
        public OrderState Status { get; set; } = OrderState.Pending;
        public Address ShippingAddress { get; set; } = null!;
        public decimal SubTotal { get; set; }

        // Navigational Property [ONE]
        public DeliveryMethod? DeliveryMethod { get; set; }

        // Navigational Property [MANY]
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

        public decimal GetTotal()
            => SubTotal + (DeliveryMethod?.Cost ?? 0);

        public string PaymentIntenId { get; set; } = string.Empty;

    }
}
