using SnapShop.Core.Entities.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Specifications.OrderSpecs
{
    public class OrderWithPaymentIntentSpecifications : BaseSpecifications<Order>
    {
        public OrderWithPaymentIntentSpecifications(string paymentIntentId)
            :base (O => O.PaymentIntenId == paymentIntentId)
        {
            
        }
    }
}
