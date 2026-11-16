using FluentAssertions;
using Moq;
using SnapShop.Application;
using SnapShop.Core;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using SnapShop.Core.Repositories.Contract;
using SnapShop.Core.Services.Contract;
using SnapShop.Core.Specifications;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SnapShop.UnitTests
{
    public class OrderServiceTests
    {
        private readonly Mock<IBasketRepository> _basketRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly OrderService _sut; // System Under Test

        public OrderServiceTests()
        {
            _basketRepoMock = new Mock<IBasketRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _paymentServiceMock = new Mock<IPaymentService>();

            _sut = new OrderService(
                _basketRepoMock.Object,
                _unitOfWorkMock.Object,
                _paymentServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateOrderAsync_WhenBasketNotFound_ReturnsNull()
        {
            // Arrange
            _basketRepoMock.Setup(r => r.GetBasketAsync(It.IsAny<string>()))
                .ReturnsAsync((CustomerBasket?)null);

            var address = new Address { FirstName = "Ahmed", LastName = "Ali", Street = "Main St", City = "Cairo", Country = "Egypt" };

            // Act
            var result = await _sut.CreateOrderAsync("buyer@test.com", "basket_123", 1, address);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateOrderAsync_WhenBasketHasNoItems_ReturnsNull()
        {
            // Arrange
            var emptyBasket = new CustomerBasket { Id = "basket_123", Items = new List<BasketItem>() };
            _basketRepoMock.Setup(r => r.GetBasketAsync("basket_123"))
                .ReturnsAsync(emptyBasket);

            var address = new Address { FirstName = "Ahmed", LastName = "Ali", Street = "Main St", City = "Cairo", Country = "Egypt" };

            // Act
            var result = await _sut.CreateOrderAsync("buyer@test.com", "basket_123", 1, address);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateOrderAsync_WhenDeliveryMethodNotFound_ReturnsNull()
        {
            // Arrange
            var basket = new CustomerBasket
            {
                Id = "basket_123",
                Items = new List<BasketItem>
                {
                    new BasketItem { Id = 1, Name = "Shoe", Price = 250m, Quantity = 2 }
                }
            };
            _basketRepoMock.Setup(r => r.GetBasketAsync("basket_123"))
                .ReturnsAsync(basket);

            var productMock = new Product { Name = "Shoe", Price = 250m, PictureUrl = "shoe.png" };
            var productRepoMock = new Mock<IGenericRepository<Product>>();
            productRepoMock.Setup(r => r.GetAsync(1)).ReturnsAsync(productMock);

            var deliveryRepoMock = new Mock<IGenericRepository<DeliveryMethod>>();
            deliveryRepoMock.Setup(r => r.GetAsync(999)).ReturnsAsync((DeliveryMethod?)null);

            _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<DeliveryMethod>()).Returns(deliveryRepoMock.Object);

            var address = new Address { FirstName = "Sara", LastName = "Hassan", Street = "Pyramids St", City = "Giza", Country = "Egypt" };

            // Act
            var result = await _sut.CreateOrderAsync("buyer@test.com", "basket_123", 999, address);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateOrderAsync_WithValidBasketAndDelivery_ReturnsCreatedOrder()
        {
            // Arrange
            var basket = new CustomerBasket
            {
                Id = "basket_valid",
                PaymentInentId = "pi_test_123",
                Items = new List<BasketItem>
                {
                    new BasketItem { Id = 10, Name = "Shirt", Price = 150m, Quantity = 2 }
                }
            };
            _basketRepoMock.Setup(r => r.GetBasketAsync("basket_valid"))
                .ReturnsAsync(basket);

            var product = new Product { Name = "Shirt", Price = 150m, PictureUrl = "shirt.jpg" };
            var productRepoMock = new Mock<IGenericRepository<Product>>();
            productRepoMock.Setup(r => r.GetAsync(10)).ReturnsAsync(product);

            var deliveryMethod = new DeliveryMethod { Cost = 40m, ShortName = "Express", DeliveryTime = "24h" };
            var deliveryRepoMock = new Mock<IGenericRepository<DeliveryMethod>>();
            deliveryRepoMock.Setup(r => r.GetAsync(1)).ReturnsAsync(deliveryMethod);

            var orderRepoMock = new Mock<IGenericRepository<Order>>();
            orderRepoMock.Setup(r => r.GetEntityWithSpec(It.IsAny<ISpecifications<Order>>()))
                .ReturnsAsync((Order?)null);
            orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.Repository<Product>()).Returns(productRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<DeliveryMethod>()).Returns(deliveryRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            var address = new Address { FirstName = "Omar", LastName = "Khaled", Street = "El-Nasr St", City = "Cairo", Country = "Egypt" };

            // Act
            var result = await _sut.CreateOrderAsync("omar@test.com", "basket_valid", 1, address);

            // Assert
            result.Should().NotBeNull();
            result!.BuyerEmail.Should().Be("omar@test.com");
            result.SubTotal.Should().Be(300m); // 150 * 2
            result.GetTotal().Should().Be(340m); // 300 + 40
            result.Items.Should().HaveCount(1);
            result.Status.Should().Be(OrderState.Pending);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WhenOrderExists_UpdatesAndReturnsOrder()
        {
            // Arrange
            var existingOrder = new Order
            {
                BuyerEmail = "customer@test.com",
                Status = OrderState.Pending
            };

            var orderRepoMock = new Mock<IGenericRepository<Order>>();
            orderRepoMock.Setup(r => r.GetEntityWithSpec(It.IsAny<ISpecifications<Order>>()))
                .ReturnsAsync(existingOrder);
            orderRepoMock.Setup(r => r.Update(It.IsAny<Order>()));

            _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _sut.UpdateOrderStatusAsync(5, OrderState.Shipped);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(OrderState.Shipped);
            orderRepoMock.Verify(r => r.Update(existingOrder), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WhenOrderNotFound_ReturnsNull()
        {
            // Arrange
            var orderRepoMock = new Mock<IGenericRepository<Order>>();
            orderRepoMock.Setup(r => r.GetEntityWithSpec(It.IsAny<ISpecifications<Order>>()))
                .ReturnsAsync((Order?)null);

            _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);

            // Act
            var result = await _sut.UpdateOrderStatusAsync(999, OrderState.Delivered);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllOrdersAsync_ReturnsListOfOrders()
        {
            // Arrange
            var sampleOrders = new List<Order>
            {
                new Order { BuyerEmail = "user1@test.com", SubTotal = 200m },
                new Order { BuyerEmail = "user2@test.com", SubTotal = 450m }
            };

            var orderRepoMock = new Mock<IGenericRepository<Order>>();
            orderRepoMock.Setup(r => r.GetAllAsyncWithSpec(It.IsAny<ISpecifications<Order>>()))
                .ReturnsAsync(sampleOrders);

            _unitOfWorkMock.Setup(u => u.Repository<Order>()).Returns(orderRepoMock.Object);

            // Act
            var result = await _sut.GetAllOrdersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(o => o.BuyerEmail == "user1@test.com");
            result.Should().Contain(o => o.BuyerEmail == "user2@test.com");
        }
    }
}
