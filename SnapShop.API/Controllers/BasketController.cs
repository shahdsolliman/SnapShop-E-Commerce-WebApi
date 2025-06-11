using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnapShop.API.Dtos;
using SnapShop.API.Errors;
using SnapShop.Core.Entities;
using SnapShop.Core.Repositories.Contract;

namespace SnapShop.API.Controllers
{
    public class BasketController : BaseAPIController
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketController(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        [HttpGet]   // GET: /api/basket?id=1

        public async Task<ActionResult<CustomerBasket>> GetBasket(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            return Ok(basket ?? new CustomerBasket(id));      // Get Basket Or Recreate
        }

        [HttpPost]   // POST: /api/basket
        public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasketDto basket)
        {
            var mappedBasket = _mapper.Map<CustomerBasketDto, CustomerBasket>(basket); 
            var updatedOrCreatedBasket = await _basketRepository.UpdateBasketAsync(mappedBasket);   // Create Basket For First Time
            return Ok(updatedOrCreatedBasket);
        }

        [HttpDelete("{id}")]   // DELETE: /api/basket/1
        public async Task<bool> DeleteBasket(string id)
        {
            return await _basketRepository.DeleteBasketAsync(id);
        }

    }
}
