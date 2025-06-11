using Microsoft.Extensions.Configuration;
using SnapShop.Core;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using SnapShop.Core.Repositories.Contract;
using SnapShop.Core.Services.Contract;
using SnapShop.Core.Specifications.Order_Specs;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Application
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(
            IConfiguration configuration,
            IBasketRepository basketRepo,
            IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
        }
        public async Task<CustomerBasket?> CreateOrUpdatePaymentItent(string basketId)
        {
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];

            var basket = await _basketRepo.GetBasketAsync(basketId);
            if (basket == null) return null;

            var shippingPrice = 0m;
            if (basket.DelieveryMethod.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(basket.DelieveryMethod.Value);
                basket.ShippingPrice = deliveryMethod.Cost;
                shippingPrice = deliveryMethod.Cost;
            }

            if (basket?.Items.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Core.Entities.Product>().GetAsync(item.Id);
                    if (item.Price != product.Price)
                        item.Price = product.Price;
                }
            }

            PaymentIntent paymentIntent ;
            PaymentIntentService paymentIntentService = new PaymentIntentService();
            if (string.IsNullOrEmpty(basket.PaymentInentId))   // Create New PaymentInentId
            {
                var createOptions = new PaymentIntentCreateOptions
                {
                    Amount = (long)((basket.Items.Sum(item => item.Price * item.Quantity) + shippingPrice) * 100), // Convert to cents
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" }
                };
                paymentIntent = await paymentIntentService.CreateAsync(createOptions);
                basket.PaymentInentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;

            }
            else // Update Existing PaymentInentId
            {
                var updatedOptions = new PaymentIntentUpdateOptions
                {
                    Amount = (long)((basket.Items.Sum(item => item.Price * item.Quantity) + shippingPrice) * 100), // Convert to cents
                };
                paymentIntent = await paymentIntentService.UpdateAsync(basket.PaymentInentId, updatedOptions);
            }

            var updatedBasket = await _basketRepo.UpdateBasketAsync(basket);
            return basket;
        }

        public async Task<Order> UpdatePaymentIntentToSucceedOrFailed(string paymentIntentId, bool isSucceeded)
        {
            var spec = new OrderWithPaymentIntentSpecifications(paymentIntentId);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
            if (isSucceeded)
                order.Status = OrderState.PaymentReceived;
            else
                order.Status = OrderState.PaymentFailed;

            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.DisposeAsync();
            return order;

        }
    }
}
