using SnapShop.Core;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using SnapShop.Core.Repositories.Contract;
using SnapShop.Core.Services.Contract;
using SnapShop.Core.Specifications.Order_Specs;

namespace SnapShop.Application
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        //private readonly IGenericRepository<Product> _productRepo;
        //private readonly IGenericRepository<DeliveryMethod> _deliveryMethodsRepo;
        //private readonly IGenericRepository<Order> _orderRepo;

        public OrderService(
            IBasketRepository basketRepo,
            IUnitOfWork unitOfWork,
            IPaymentService paymentService
            //IGenericRepository<Product> productRepo,
            //IGenericRepository<DeliveryMethod> deliveryMethodsRepo,
            //IGenericRepository<Order> orderRepo
            )
        {
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            //_productRepo = productRepo;
            //_deliveryMethodsRepo = deliveryMethodsRepo;
            //_orderRepo = orderRepo;
        }
        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int dileveryMethodId, Address shippingAddress)
        {
            // 1. Get basket from the basketRepo
            var basket = await _basketRepo.GetBasketAsync(basketId);

            // 2. Get selctedItems at basket from the productRepo
            var orderItems = new List<OrderItem>();
            
            if(basket?.Items?.Count() > 0)
            {
                foreach(var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    var productItemOrdered = new ProductItemOrder(item.Id, product.Name, product.PictureUrl);
                    var orderItem = new OrderItem(productItemOrdered, product.Price, item.Quantity);
                    orderItems.Add(orderItem);
                }
            }
             
            // 3. Calculate Subtotal
            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            // 4. Get delivery method from the deliverymethodRepo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(dileveryMethodId);

            var ordersRepo = _unitOfWork.Repository<Order>();
            var orderSpecs = new OrderWithPaymentIntentSpecifications(basket.PaymentInentId);
            var existingOrder = await ordersRepo.GetEntityWithSpec(orderSpecs);

            if(existingOrder != null)
            {
                ordersRepo.Delete(existingOrder);
                await _paymentService.CreateOrUpdatePaymentItent(basket.Id);
            }
            // 5. Create Order
            var order = new Order(buyerEmail, shippingAddress, subTotal, deliveryMethod, orderItems, basket.PaymentInentId);
            await _unitOfWork.Repository<Order>().AddAsync(order);

            // 6. Save to Database [TODO]
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return null;
            

            return order;

        }


        public async Task<Order?> GetOrderByIdForUserAsync(int orderId, string buyerEmail)
            => await _unitOfWork.Repository<Order>().GetEntityWithSpec(new OrderSpecifications(orderId, buyerEmail));

        public async Task<IReadOnlyList<Order?>> GetOrdersForUserAsync(string buyerEmail)
            => await _unitOfWork.Repository<Order>().GetAllAsyncWithSpec(new OrderSpecifications(buyerEmail));
        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
            => await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();

    }
}
