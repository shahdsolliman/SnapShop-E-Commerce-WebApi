using FluentAssertions;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using System.Collections.Generic;
using Xunit;

namespace SnapShop.UnitTests
{
    public class EntitiesTests
    {
        [Fact]
        public void Order_GetTotal_ReturnsSubTotalPlusDeliveryMethodCost()
        {
            // Arrange
            var deliveryMethod = new DeliveryMethod { Cost = 50m, ShortName = "Express" };
            var order = new Order
            {
                SubTotal = 300m,
                DeliveryMethod = deliveryMethod
            };

            // Act
            var total = order.GetTotal();

            // Assert
            total.Should().Be(350m);
        }

        [Fact]
        public void Order_GetTotal_WhenDeliveryMethodNull_ReturnsSubTotal()
        {
            // Arrange
            var order = new Order
            {
                SubTotal = 400m,
                DeliveryMethod = null
            };

            // Act
            var total = order.GetTotal();

            // Assert
            total.Should().Be(400m);
        }

        [Fact]
        public void Order_DefaultStatus_IsPending()
        {
            // Act
            var order = new Order();

            // Assert
            order.Status.Should().Be(OrderState.Pending);
        }

        [Fact]
        public void CustomerBasket_Initialization_ItemsListIsNotNull()
        {
            // Act
            var basket = new CustomerBasket();

            // Assert
            basket.Items.Should().NotBeNull();
            basket.Items.Should().BeEmpty();
        }

        [Fact]
        public void OrderItem_Properties_AreSetCorrectly()
        {
            // Arrange
            var productItem = new ProductItemOrder(1, "Sneaker", "sneaker.jpg");
            var item = new OrderItem(productItem, 250m, 3);

            // Assert
            item.Price.Should().Be(250m);
            item.Quantity.Should().Be(3);
            item.Product.ProductId.Should().Be(1);
            item.Product.ProductName.Should().Be("Sneaker");
            item.Product.PictureUrl.Should().Be("sneaker.jpg");
        }
    }
}
