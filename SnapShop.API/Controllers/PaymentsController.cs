using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnapShop.API.Errors;
using SnapShop.Core.Entities;
using SnapShop.Core.Entities.OrderAggregate;
using SnapShop.Core.Services.Contract;
using Stripe;
using System.Net;

namespace SnapShop.API.Controllers
{
    public class PaymentsController : BaseAPIController
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger,
            IConfiguration configuration
            )
        {
            _paymentService = paymentService;
            _logger = logger;
            _configuration = configuration;
        }

        [Authorize]
        [ProducesResponseType(typeof(CustomerBasket), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("{basketId}")]    // POST: /api/Payments/{basketId}
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentItent(basketId);
            if (basket == null) return BadRequest(new ApiResponse(400, "Problem with your basket."));
            return Ok(basket);
        }

        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _configuration["StripeSettings:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret);

                PaymentIntent intent;
                Order order;

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        intent = (PaymentIntent)stripeEvent.Data.Object;
                        order = await _paymentService.UpdatePaymentIntentToSucceedOrFailed(intent.Id, true);
                        _logger.LogInformation("Payment Succeeded: {PaymentIntentId}", intent.Id);
                        break;
                    case "payment_intent.payment_failed":
                        intent = (PaymentIntent)stripeEvent.Data.Object;
                        order = await _paymentService.UpdatePaymentIntentToSucceedOrFailed(intent.Id, false);
                        _logger.LogInformation("Payment Failed: {PaymentIntentId}", intent.Id);
                        break;
                    default:
                        _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed.");
                return BadRequest(new ApiResponse(400, "Invalid Stripe webhook signature."));
            }
        }
    }
}
