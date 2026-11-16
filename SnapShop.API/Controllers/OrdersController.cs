using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnapShop.API.Dtos;
using SnapShop.API.Errors;
using SnapShop.Core.Entities.OrderAggregate;
using SnapShop.Core.Services.Contract;
using System.Security.Claims;

namespace SnapShop.API.Controllers
{
    public class OrdersController : BaseAPIController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrdersController(
            IOrderService orderService,
            IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        /// <summary>
        /// Create a new order (Supports registered users and guest customers).
        /// </summary>
        [AllowAnonymous]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost]  // POST: /api/orders
        public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto orderDto)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(buyerEmail))
            {
                if (!string.IsNullOrWhiteSpace(orderDto.BuyerEmail))
                    buyerEmail = orderDto.BuyerEmail.Trim();
                else if (!string.IsNullOrWhiteSpace(orderDto.Phone))
                    buyerEmail = $"customer_{orderDto.Phone.Trim()}@snapshop.local";
                else
                    buyerEmail = "customer@snapshop.local";
            }

            var address = _mapper.Map<AddressDto, Address>(orderDto.ShippingAddress);

            // If phone is provided, make sure it's prominent in the address
            if (!string.IsNullOrWhiteSpace(orderDto.Phone) && !address.Street.Contains(orderDto.Phone))
            {
                address.Street = $"{address.Street} (📱 {orderDto.Phone.Trim()})";
            }

            var order = await _orderService.CreateOrderAsync(buyerEmail, orderDto.BasketId, orderDto.DeliveryMethodId, address);

            if (order is null)
                return BadRequest(new ApiResponse(400, "Failed to create order. Please check your basket and delivery method."));

            return StatusCode(StatusCodes.Status201Created, _mapper.Map<Order, OrderToReturnDto>(order));
        }

        /// <summary>
        /// Public Order Tracking by Order ID (No login required for shoppers).
        /// </summary>
        [AllowAnonymous]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("track/{id}")]    // GET: /api/orders/track/{id}
        public async Task<ActionResult<OrderToReturnDto>> TrackOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order is null)
                return NotFound(new ApiResponse(404, "الطلب غير موجود. يرجى التأكد من رقم الطلب والرمز المدخل."));

            return Ok(_mapper.Map<Order, OrderToReturnDto>(order));
        }

        [Authorize]
        [ProducesResponseType(typeof(IReadOnlyList<OrderToReturnDto>), StatusCodes.Status200OK)]
        [HttpGet]   // GET: /api/orders
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser()
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var orders = await _orderService.GetOrdersForUserAsync(buyerEmail);
            return Ok(_mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(orders));
        }

        [Authorize]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]   // GET: /api/orders/{id}
        public async Task<ActionResult<OrderToReturnDto>> GetOrderForUser(int id)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
            var order = await _orderService.GetOrderByIdForUserAsync(id, buyerEmail);
            if (order is null) return NotFound(new ApiResponse(404));
            return Ok(_mapper.Map<OrderToReturnDto>(order));
        }

        [AllowAnonymous]
        [ProducesResponseType(typeof(IReadOnlyList<DeliveryMethod>), StatusCodes.Status200OK)]
        [HttpGet("deliveryMethods")]    // GET: /api/orders/deliveryMethods
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            var deliveryMethods = await _orderService.GetDeliveryMethodsAsync();
            return Ok(deliveryMethods);
        }

        /// <summary>
        /// Get all customer orders (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IReadOnlyList<OrderToReturnDto>), StatusCodes.Status200OK)]
        [HttpGet("all")]    // GET: /api/orders/all
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(_mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(orders));
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpPut("{id}/status")]    // PUT: /api/orders/{id}/status?status=Accepted
        public async Task<ActionResult<OrderToReturnDto>> UpdateOrderStatus(int id, [FromQuery] OrderState status)
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, status);
            if (order is null) return NotFound(new ApiResponse(404, "Order not found"));
            return Ok(_mapper.Map<Order, OrderToReturnDto>(order));
        }
    }
}
