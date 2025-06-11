using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;

namespace SnapShop.Infrastructure.Data
{
    public class StoreContextSeed
    {
        public async static Task SeedAsync(StoreContext _dbContext)
        {


            if (!_dbContext.ProductCategories.Any())
            {
                var categoriesData = File.ReadAllText("../SnapShop.Infrastructure/Data/DataSeeding/categories.json");
                var categories = JsonSerializer.Deserialize<List<ProductCategory>>(categoriesData);
                if (categories is not null)
                {
                    foreach (var item in categories)
                    {
                        _dbContext.Set<ProductCategory>().Add(item);
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (!_dbContext.ProductBrands.Any())
            {
                var brandsData = File.ReadAllText("../SnapShop.Infrastructure/Data/DataSeeding/brands.json");
                var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandsData);
                if (brands is not null)
                {
                    foreach (var item in brands)
                    {
                        _dbContext.Set<ProductBrand>().Add(item);
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (!_dbContext.Products.Any())
            {
                var productsData = File.ReadAllText("../SnapShop.Infrastructure/Data/DataSeeding/products.json");
                var products = JsonSerializer.Deserialize<List<Product>>(productsData);
                if (products is not null)
                {
                    foreach (var item in products)
                    {
                        _dbContext.Set<Product>().Add(item);
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (!_dbContext.DeliveryMethods.Any())
            {
                var deliveryMethodsData = File.ReadAllText("../SnapShop.Infrastructure/Data/DataSeeding/delivery.json");
                var deliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(deliveryMethodsData);
                if (deliveryMethods is not null)
                {
                    foreach (var deliveryMethod in deliveryMethods)
                    {
                        _dbContext.Set<DeliveryMethod>().Add(deliveryMethod);
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
