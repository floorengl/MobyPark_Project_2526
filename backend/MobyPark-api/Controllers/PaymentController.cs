using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("payments")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _payment;
    public PaymentController(IPaymentService payment) => _payment = payment;

    // GET /PaymentsBetween
    [HttpGet("PaymentsBetween")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentsBetween(DateTime start, DateTime end)
    {
        if (start > end)
        {
            return BadRequest("Start date must be earlier than end date");
        }

        var payments = await _payment.GetPaymentsBetweenAsync(start, end);

        if (payments == null || !payments.Any())
        {
            return NotFound("No payments found for the specified date range");
        }
        
        return Ok(payments);
    }
}