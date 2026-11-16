using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Entities.OrderAggregate
{
    public enum OrderState
    {
        [EnumMember(Value = "Pending")]
        Pending,

        [EnumMember(Value = "Accepted")]
        Accepted,

        [EnumMember(Value = "Payment Received")]
        PaymentReceived,

        [EnumMember(Value = "Payment Failed")]
        PaymentFailed,

        [EnumMember(Value = "Shipped")]
        Shipped,

        [EnumMember(Value = "Delivered")]
        Delivered,

        [EnumMember(Value = "Cancelled")]
        Cancelled
    }
}
