using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnapShop.API.Dtos;
using SnapShop.API.Errors;
using SnapShop.Core.Entities;
using SnapShop.Core.Repositories.Contract;

namespace SnapShop.API.Controllers
{
    [Authorize]
    public class BasketController : BaseAPIController
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketController(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        [ProducesResponseType(typeof(CustomerBasket), StatusCodes.Status200OK)]
        [HttpGet]   // GET: /api/basket?id=xxx
        public async Task<ActionResult<CustomerBasket>> GetBasket(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            return Ok(basket ?? new CustomerBasket(id));
        }

        [ProducesResponseType(typeof(CustomerBasket), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost]  // POST: /api/basket
        public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasketDto basket)
        {
            var mappedBasket = _mapper.Map<CustomerBasketDto, CustomerBasket>(basket);
            var updatedOrCreatedBasket = await _basketRepository.UpdateBasketAsync(mappedBasket);
            if (updatedOrCreatedBasket is null) return BadRequest(new ApiResponse(400, "Failed to update basket."));
            return Ok(updatedOrCreatedBasket);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpDelete("{id}")]    // DELETE: /api/basket/{id}
        public async Task<ActionResult> DeleteBasket(string id)
        {
            var deleted = await _basketRepository.DeleteBasketAsync(id);
            if (!deleted) return NotFound(new ApiResponse(404, "Basket not found."));
            return NoContent();
        }
    }
}
