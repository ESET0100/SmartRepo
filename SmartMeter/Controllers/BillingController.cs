using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
using SmartMeter.Models;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BillingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Billing>>> GetBillings()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }
            return await _context.Billings
                .Include(b => b.Consumer)
                .Include(b => b.Meter)
                .Include(b => b.Arrears)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Billing>> GetBilling(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var billing = await _context.Billings
                .Include(b => b.Consumer)
                .Include(b => b.Meter)
                .Include(b => b.Arrears)
                .FirstOrDefaultAsync(b => b.BillId == id);

            if (billing == null) return NotFound();
            return billing;
        }

        [HttpPost]
        public async Task<ActionResult<Billing>> PostBilling(BillingDto billingDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var billing = new Billing
            {
                ConsumerId = billingDto.ConsumerId,
                MeterId = billingDto.MeterId,
                BillingPeriodStart = billingDto.BillingPeriodStart,
                BillingPeriodEnd = billingDto.BillingPeriodEnd,
                TotalUnitsConsumed = billingDto.TotalUnitsConsumed,
                BaseAmount = billingDto.BaseAmount,
                TaxAmount = billingDto.TaxAmount,
                DueDate = billingDto.DueDate,
                GeneratedAt = DateTime.UtcNow
            };

            _context.Billings.Add(billing);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBilling), new { id = billing.BillId }, billing);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBilling(long id, BillingDto billingDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            if (billingDto.BillId.HasValue && id != billingDto.BillId.Value)
                return BadRequest("ID mismatch");

            var existingBilling = await _context.Billings.FindAsync(id);
            if (existingBilling == null)
                return NotFound();

            // Update fields
            existingBilling.ConsumerId = billingDto.ConsumerId;
            existingBilling.MeterId = billingDto.MeterId;
            existingBilling.BillingPeriodStart = billingDto.BillingPeriodStart;
            existingBilling.BillingPeriodEnd = billingDto.BillingPeriodEnd;
            existingBilling.TotalUnitsConsumed = billingDto.TotalUnitsConsumed;
            existingBilling.BaseAmount = billingDto.BaseAmount;
            existingBilling.TaxAmount = billingDto.TaxAmount;
            existingBilling.DueDate = billingDto.DueDate;
            existingBilling.PaymentStatus = billingDto.PaymentStatus;
            existingBilling.PaidDate = billingDto.PaidDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillingExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilling(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var billing = await _context.Billings.FindAsync(id);
            if (billing == null) return NotFound();
            _context.Billings.Remove(billing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool BillingExists(long id) => _context.Billings.Any(e => e.BillId == id);
    }
}