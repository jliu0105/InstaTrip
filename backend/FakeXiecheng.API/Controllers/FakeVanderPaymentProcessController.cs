using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaTrip.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FakeVanderPaymentProcessController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(
            [FromQuery] Guid orderNumber,
            [FromQuery] bool returnFault = false
        )
        {
            await Task.Delay(3000);

            if (returnFault)
            {
                return Ok(new
                {
                    id = Guid.NewGuid(),
                    created = DateTime.UtcNow,
                    approved = false,
                    message = "Reject",
                    payment_metohd = "credit card payment",
                    order_number = orderNumber,
                    card = new { 
                        card_type = "credit card",
                        last_four = "1234"
                    }
                }) ;
            }

            return Ok(new
            {
                id = Guid.NewGuid(),
                created = DateTime.UtcNow,
                approved = true,
                message = "Reject",
                payment_metohd = "credit card payment",
                order_number = orderNumber,
                card = new
                {
                    card_type = "credit card",
                    last_four = "1234"
                }
            });
        }
    }
}
