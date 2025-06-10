using MediatR;
using Order.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Queries.GetOrders;
using Order.Application.Commands.RegisterOrder;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all orders or filters by orderId or externalId
        /// </summary>
        /// <param name="orderId">Optional order ID to filter by</param>
        /// <param name="externalId">Optional external ID to filter by</param>
        /// <returns>List of orders</returns>
        [HttpGet("orders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrders([FromQuery] Guid? orderId, [FromQuery] string? externalId)
        {
            try
            {
                var query = new GetOrdersQuery
                {
                    OrderId = orderId,
                    ExternalId = externalId
                };

                var result = await _mediator.Send(query);

                if (!result.Success)
                    return StatusCode(500, result);

                if (result.Orders.Count == 0)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving orders" });
            }
        }

        /// <summary>
        /// Registers a new order
        /// </summary>
        /// <param name="command">Order information</param>
        /// <returns>Created order information</returns>
        [HttpPost("register-order")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterOrder([FromBody] RegisterOrderCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    if (result.Message.Contains("Duplicate"))
                        return Conflict(result);

                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetOrders), new { orderId = result.OrderId }, result);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors.ToResponse());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering order");
                return StatusCode(500, new { success = false, message = "An error occurred while registering the order" });
            }
        }
    }
}
