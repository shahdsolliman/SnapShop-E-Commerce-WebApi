using SnapShop.Core.Entities.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Specifications.OrderSpecs
{
    public class OrderSpecifications : BaseSpecifications<Order>
    {
        public OrderSpecifications(string buyerEmail)
            :base(O => O.BuyerEmail == buyerEmail)
        {
            Includes.Add(O => O.DeliveryMethod);
            Includes.Add(O => O.Items);
            AddOrderByDesc(O => O.Date);
        }

        // Admin: get all orders
        public OrderSpecifications()
            : base()
        {
            Includes.Add(O => O.DeliveryMethod);
            Includes.Add(O => O.Items);
            AddOrderByDesc(O => O.Date);
        }

        // Admin: get order by id
        public OrderSpecifications(int orderId)
            : base(O => O.Id == orderId)
        {
            Includes.Add(O => O.DeliveryMethod);
            Includes.Add(O => O.Items);
        }

        public OrderSpecifications(int orderId, string buyerEmail)
            :base(O => O.Id == orderId && O.BuyerEmail == buyerEmail)
        {
            Includes.Add(O => O.DeliveryMethod);
            Includes.Add(O => O.Items);
        }
    }
}
