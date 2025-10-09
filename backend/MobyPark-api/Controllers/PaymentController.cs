using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("payments")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _payment;
    public PaymentController(IPaymentService payment) => _payment = payment;


    [HttpGet("PaymentsBetween")]
    [Authorize]
    public async Task<IActionResult> PaymentsBetween(DateTime start, DateTime end)
    {
        var list = await _payment.GetPaymentsBetweenAsync(start, end);
        return Ok(list);
    }
}