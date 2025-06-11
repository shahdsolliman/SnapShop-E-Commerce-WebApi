using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Proxies.Internal;
using SnapShop.API.Dtos;
using SnapShop.API.Errors;
using SnapShop.API.Helpers;
using SnapShop.Core;
using SnapShop.Core.Entities;
using SnapShop.Core.Repositories.Contract;
using SnapShop.Core.Specifications.Product_Specs;
using SnapShop.Infrastructure;
using SnapShop.Infrastructure.Data;

namespace SnapShop.API.Controllers
{
    public class ProductsController : BaseAPIController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        //private readonly IGenericRepository<Product> _productRepo;
        //private readonly IGenericRepository<ProductBrand> _brandsRepo;
        //private readonly IGenericRepository<ProductCategory> _categoriesRepo;

        public ProductsController(
            IMapper mapper,
            IUnitOfWork unitOfWork
            //IGenericRepository<Product> productRepo,
            //Repository<ProductBrand> brandsRepo
            /*IGenericRepository<ProductCategory> categoriesRepo*/)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            //_productRepo = productRepo;
            //_brandsRepo = brandsRepo;
            //_categoriesRepo = categoriesRepo;
        }

        [ProducesResponseType(typeof(Pagination<ProductToReturnDto>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams specParams)
        {
            var spec = new ProductWithBrandAndCategorySpecifications(specParams);
            var products = await _unitOfWork.Repository<Product>().GetAllAsyncWithSpec(spec);

            var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

            var countSpec = new ProductsWithFilterationForCountSpecifications(specParams);
            var count = await _unitOfWork.Repository<Product>().GetCountAsync(countSpec);

            return Ok(new Pagination<ProductToReturnDto>(specParams.PageIndex, specParams.PageSize, count, data));
        }

        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var spec = new ProductWithBrandAndCategorySpecifications(id);
            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
            if (product == null)
                return NotFound(new ApiResponse(404));

            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));

        }

        [ProducesResponseType(typeof(IReadOnlyList<ProductBrand>), StatusCodes.Status200OK)]
        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetBrands()
        {
            var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
            return Ok(brands);
        }

        [ProducesResponseType(typeof(IReadOnlyList<ProductCategory>), StatusCodes.Status200OK)]
        [HttpGet("categories")]
        public async Task<ActionResult<IReadOnlyList<ProductCategory>>> GetCategories()
        {
            var categories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();
            return Ok(categories);
        }

    }
}
