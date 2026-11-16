using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Core.Services.Contract
{
    public interface IPaymentService
    {
        Task<CustomerBasket?> CreateOrUpdatePaymentItent(string basketId);
        Task<Order?> UpdatePaymentIntentToSucceedOrFailed(string paymentIntentId, bool isSucceeded);
    }
}
