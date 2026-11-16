using SnapShop.Core;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using SnapShop.Core.Repositories.Contract;
using SnapShop.Core.Services.Contract;
using SnapShop.Core.Specifications.OrderSpecs;

namespace SnapShop.Application
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public OrderService(
            IBasketRepository basketRepo,
            IUnitOfWork unitOfWork,
            IPaymentService paymentService)
        {
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }

        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress)
        {
            // 1. Get basket — return null if not found or empty
            var basket = await _basketRepo.GetBasketAsync(basketId);

            if (basket == null || basket.Items == null || basket.Items.Count == 0)
                return null;

            // 2. Build order items using server-side prices (not client-provided)
            var orderItems = new List<OrderItem>();

            foreach (var item in basket.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);

                if (product == null) continue;  // Skip discontinued products

                var productItemOrdered = new ProductItemOrder(item.Id, product.Name, product.PictureUrl);
                var orderItem = new OrderItem(productItemOrdered, product.Price, item.Quantity);
                orderItems.Add(orderItem);
            }

            if (orderItems.Count == 0) return null;

            // 3. Calculate subtotal using server-side prices
            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            // 4. Get delivery method
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(deliveryMethodId);
            if (deliveryMethod == null) return null;

            // 5. Check for existing order with same PaymentIntentId and delete it (only if PaymentIntentId is set)
            var ordersRepo = _unitOfWork.Repository<Order>();
            if (!string.IsNullOrEmpty(basket.PaymentInentId))
            {
                var orderSpecs = new OrderWithPaymentIntentSpecifications(basket.PaymentInentId);
                var existingOrder = await ordersRepo.GetEntityWithSpec(orderSpecs);

                if (existingOrder != null)
                {
                    ordersRepo.Delete(existingOrder);
                    await _paymentService.CreateOrUpdatePaymentItent(basket.Id);
                }
            }

            // 6. Create and persist new order
            var order = new Order(buyerEmail, shippingAddress, subTotal, deliveryMethod, orderItems, basket.PaymentInentId ?? string.Empty);
            await _unitOfWork.Repository<Order>().AddAsync(order);

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0) return null;

            return order;
        }

        public async Task<Order?> GetOrderByIdForUserAsync(int orderId, string buyerEmail)
            => await _unitOfWork.Repository<Order>().GetEntityWithSpec(new OrderSpecifications(orderId, buyerEmail));

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var orders = await _unitOfWork.Repository<Order>().GetAllAsyncWithSpec(new OrderSpecifications(buyerEmail));
            return orders ?? new List<Order>();
        }

        public async Task<IReadOnlyList<Order>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Repository<Order>().GetAllAsyncWithSpec(new OrderSpecifications());
            return orders ?? new List<Order>();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
            => await _unitOfWork.Repository<Order>().GetEntityWithSpec(new OrderSpecifications(orderId));

        public async Task<Order?> UpdateOrderStatusAsync(int orderId, OrderState status)
        {
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpec(new OrderSpecifications(orderId));
            if (order == null) return null;

            order.Status = status;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
            => await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
    }
}
