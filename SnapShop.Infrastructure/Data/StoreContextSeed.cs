using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;

namespace SnapShop.Infrastructure.Data
{
    public class StoreContextSeed
    {
        public static async Task SeedAsync(StoreContext _dbContext)
        {
            // ── 1. Categories ──
            if (!_dbContext.ProductCategories.Any())
            {
                var filePath = FindSeedFile("categories.json");
                if (filePath != null)
                {
                    var data = await File.ReadAllTextAsync(filePath);
                    var categories = JsonSerializer.Deserialize<List<ProductCategory>>(data);
                    if (categories != null)
                    {
                        foreach (var item in categories)
                            _dbContext.ProductCategories.Add(item);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    _dbContext.ProductCategories.AddRange(
                        new ProductCategory { Name = "ملابس وأزياء" },
                        new ProductCategory { Name = "إلكترونيات" },
                        new ProductCategory { Name = "أحذية وحقائب" },
                        new ProductCategory { Name = "إكسسوارات" }
                    );
                    await _dbContext.SaveChangesAsync();
                }
            }

            // ── 2. Brands ──
            if (!_dbContext.ProductBrands.Any())
            {
                var filePath = FindSeedFile("brands.json");
                if (filePath != null)
                {
                    var data = await File.ReadAllTextAsync(filePath);
                    var brands = JsonSerializer.Deserialize<List<ProductBrand>>(data);
                    if (brands != null)
                    {
                        foreach (var item in brands)
                            _dbContext.ProductBrands.Add(item);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    _dbContext.ProductBrands.AddRange(
                        new ProductBrand { Name = "Nike" },
                        new ProductBrand { Name = "Adidas" },
                        new ProductBrand { Name = "Zara" },
                        new ProductBrand { Name = "Apple" },
                        new ProductBrand { Name = "Samsung" }
                    );
                    await _dbContext.SaveChangesAsync();
                }
            }

            // ── 3. Delivery Methods ──
            if (!_dbContext.DeliveryMethods.Any())
            {
                var filePath = FindSeedFile("delivery.json");
                if (filePath != null)
                {
                    var data = await File.ReadAllTextAsync(filePath);
                    var deliveryMethods = JsonSerializer.Deserialize<List<DeliveryMethod>>(data);
                    if (deliveryMethods != null)
                    {
                        foreach (var item in deliveryMethods)
                            _dbContext.DeliveryMethods.Add(item);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                else
                {
                    _dbContext.DeliveryMethods.AddRange(
                        new DeliveryMethod { ShortName = "توصيل عادي", DeliveryTime = "2-3 أيام", Description = "توصيل اقتصادي لباب المنزل", Cost = 30m },
                        new DeliveryMethod { ShortName = "توصيل سريع (Express)", DeliveryTime = "خلال 24 ساعة", Description = "أسرع توصيل للمدن الرئيسية", Cost = 60m },
                        new DeliveryMethod { ShortName = "استلام من الفرع", DeliveryTime = "نفس اليوم", Description = "استلم طلبك مجاناً من المحل مباشرة", Cost = 0m }
                    );
                    await _dbContext.SaveChangesAsync();
                }
            }

            // ── 4. Products (if JSON available) ──
            if (!_dbContext.Products.Any())
            {
                var filePath = FindSeedFile("products.json");
                if (filePath != null)
                {
                    var data = await File.ReadAllTextAsync(filePath);
                    var products = JsonSerializer.Deserialize<List<Product>>(data);
                    if (products != null)
                    {
                        foreach (var item in products)
                            _dbContext.Products.Add(item);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }
        }

        private static string? FindSeedFile(string fileName)
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Data", "DataSeeding", fileName),
                Path.Combine(AppContext.BaseDirectory, "DataSeeding", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "Data", "DataSeeding", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "SnapShop.Infrastructure", "Data", "DataSeeding", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "SnapShop.Infrastructure", "Data", "DataSeeding", fileName)
            };

            return candidates.FirstOrDefault(File.Exists);
        }
    }
}
