using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("payments")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _payment;

    public PaymentController(IPaymentService payment) => _payment = payment;

    // POST /refund
    [HttpPost("refund")]
    [Authorize]
    public async Task<IActionResult> CreateRefund(RefundPaymentDto dto)
    {
        await _payment.AddRefundAsync(dto);
        return Ok();
    }

    // PUt /UpdatePayment
    [HttpPut("UpdatePayment")]
    [Authorize]
   public async Task<IActionResult> UpdatePayment(int id, UpdatePaymentDto dto)
    {
        var payment = await _payment.UpdatePaymentAsync(id, dto);
        if (payment == null) return NotFound();

        return Ok(payment);
    }


    // POST /AddPayment
    [HttpPost("AddPayment")]
    [Authorize]
    public async Task<IActionResult> AddPaymentAsync(AddPaymentDto dto)
    {
        await _payment.AddPaymentAsync(dto);
        return Ok();
    }

    // GET /PaymentsBetween 
    [HttpGet("PaymentsBetween")]
    [Authorize]
    public async Task<IActionResult> PaymentsBetween(DateTime start, DateTime end)
    {
        if (start > end)
        {
            return BadRequest("Start date must be earlier than end date (YYYY-MM-DD)");
        }

        var payments = await _payment.GetPaymentsBetweenAsync(start, end);

        if (payments == null || !payments.Any())
        {
            return NotFound("No payments found for the specified date range");
        }

        return Ok(payments);
    }

    // GET /payments/{id}
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetPaymentById(long id)
    {
        var payment = await _payment.GetPaymentByIdAsync(id);
        if (payment == null)
            return NotFound($"Payment with ID {id} not found.");

        return Ok(payment);
    }

    // Get /GetPayments
    [HttpGet]
    public async Task<IActionResult> GetPayments()
    {
        var payments = await _payment.GetPaymentsAsync();
        if (payments == null || !payments.Any())
            return NotFound("No payments found.");

        return Ok(payments);
    }
}
