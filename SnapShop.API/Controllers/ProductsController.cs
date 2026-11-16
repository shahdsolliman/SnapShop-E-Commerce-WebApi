using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using SnapShop.API.Dtos;
using SnapShop.API.Errors;
using SnapShop.API.Helpers;
using SnapShop.Core;
using SnapShop.Core.Entities;
using SnapShop.Core.Repositories.Contract;
using SnapShop.Core.Specifications.ProductSpecs;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace SnapShop.API.Controllers
{
    public class ProductsController : BaseAPIController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public ProductsController(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment env)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _env = env;
        }

        [ProducesResponseType(typeof(Pagination<ProductToReturnDto>), StatusCodes.Status200OK)]
        [HttpGet]   // GET: /api/products
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams specParams)
        {
            var spec = new ProductWithBrandAndCategorySpecifications(specParams);
            var products = await _unitOfWork.Repository<Product>().GetAllAsyncWithSpec(spec);
            var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

            var countSpec = new ProductsWithFilterationForCountSpecifications(specParams);
            var count = await _unitOfWork.Repository<Product>().GetCountAsync(countSpec);

            return Ok(new Pagination<ProductToReturnDto>(specParams.PageIndex, specParams.PageSize, count, data));
        }

        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]   // GET: /api/products/{id}
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var spec = new ProductWithBrandAndCategorySpecifications(id);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
            if (product == null)
                return NotFound(new ApiResponse(404));

            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
        }

        [ProducesResponseType(typeof(IReadOnlyList<ProductBrand>), StatusCodes.Status200OK)]
        [HttpGet("brands")]     // GET: /api/products/brands
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetBrands()
        {
            var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
            return Ok(brands);
        }

        [ProducesResponseType(typeof(IReadOnlyList<ProductCategory>), StatusCodes.Status200OK)]
        [HttpGet("categories")]     // GET: /api/products/categories
        public async Task<ActionResult<IReadOnlyList<ProductCategory>>> GetCategories()
        {
            var categories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Create a new product with image upload (Admin only). Price is optional.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost]  // POST: /api/products
        public async Task<ActionResult<ProductToReturnDto>> CreateProduct([FromForm] ProductCreateDto createDto)
        {
            string? pictureUrl = null;
            if (createDto.Image != null && createDto.Image.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var ext = Path.GetExtension(createDto.Image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    return BadRequest(new ApiResponse(400, "Only image files (jpg, png, webp, gif) are allowed."));

                // Max 10 MB
                if (createDto.Image.Length > 10 * 1024 * 1024)
                    return BadRequest(new ApiResponse(400, "Image size must not exceed 10 MB."));

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await createDto.Image.CopyToAsync(stream);
                }
                pictureUrl = $"/uploads/{fileName}";
            }

            // Fallback for BrandId & CategoryId if not specified
            var brandId = createDto.BrandId ?? 0;
            if (brandId <= 0)
            {
                var firstBrand = (await _unitOfWork.Repository<ProductBrand>().GetAllAsync()).FirstOrDefault();
                brandId = firstBrand?.Id ?? 1;
            }

            var categoryId = createDto.CategoryId ?? 0;
            if (categoryId <= 0)
            {
                var firstCategory = (await _unitOfWork.Repository<ProductCategory>().GetAllAsync()).FirstOrDefault();
                categoryId = firstCategory?.Id ?? 1;
            }

            var product = _mapper.Map<Product>(createDto);
            product.PictureUrl = pictureUrl ?? string.Empty;
            product.Price = createDto.Price ?? 0m;
            product.Description = string.IsNullOrWhiteSpace(createDto.Description) ? createDto.Name : createDto.Description;
            product.BrandId = brandId;
            product.CategoryId = categoryId;

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            var spec = new ProductWithBrandAndCategorySpecifications(product.Id);
            var createdProduct = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
            var productDto = _mapper.Map<ProductToReturnDto>(createdProduct);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }

        /// <summary>
        /// Delete a product by ID (Admin only).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("{id}")]  // DELETE: /api/products/{id}
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var spec = new ProductWithBrandAndCategorySpecifications(id);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
            if (product == null)
                return NotFound(new ApiResponse(404));

            // Delete image file if exists
            if (!string.IsNullOrEmpty(product.PictureUrl) && product.PictureUrl.StartsWith("/uploads/"))
            {
                var filePath = Path.Combine(_env.WebRootPath, product.PictureUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _unitOfWork.Repository<Product>().Delete(product);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}
