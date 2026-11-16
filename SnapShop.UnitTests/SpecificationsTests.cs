using FluentAssertions;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using SnapShop.Core.Specifications.OrderSpecs;
using SnapShop.Core.Specifications.ProductSpecs;
using Xunit;

namespace SnapShop.UnitTests
{
    public class SpecificationsTests
    {
        [Fact]
        public void ProductWithBrandAndCategorySpecifications_WithId_SetsCriteria()
        {
            // Act
            var spec = new ProductWithBrandAndCategorySpecifications(5);

            // Assert
            spec.Criteria.Should().NotBeNull();
            spec.Includes.Should().HaveCount(2); // productBrand, productCategory
        }

        [Fact]
        public void ProductWithBrandAndCategorySpecifications_WithParams_AppliesPaging()
        {
            // Arrange
            var specParams = new ProductSpecParams
            {
                PageIndex = 2,
                PageSize = 10,
                brandId = 1,
                categoryId = 2
            };

            // Act
            var spec = new ProductWithBrandAndCategorySpecifications(specParams);

            // Assert
            spec.IsPaginationEnabled.Should().BeTrue();
            spec.Take.Should().Be(10);
            spec.Skip.Should().Be(10); // (2-1) * 10
        }

        [Fact]
        public void OrderSpecifications_ByBuyerEmail_SetsCriteriaAndOrderByDesc()
        {
            // Act
            var spec = new OrderSpecifications("customer@snapshop.com");

            // Assert
            spec.Criteria.Should().NotBeNull();
            spec.Includes.Should().HaveCount(2); // DeliveryMethod, Items
            spec.OrderByDesc.Should().NotBeNull(); // Order by Date descending
        }

        [Fact]
        public void OrderSpecifications_Parameterless_IncludesItemsAndDeliveryMethod()
        {
            // Act
            var spec = new OrderSpecifications();

            // Assert
            spec.Includes.Should().HaveCount(2);
            spec.OrderByDesc.Should().NotBeNull();
        }

        [Fact]
        public void ProductsWithFilterationForCountSpecifications_WithSearch_SetsCriteria()
        {
            // Arrange
            var specParams = new ProductSpecParams
            {
                Search = "Nike"
            };

            // Act
            var spec = new ProductsWithFilterationForCountSpecifications(specParams);

            // Assert
            spec.Criteria.Should().NotBeNull();
        }
    }
}
