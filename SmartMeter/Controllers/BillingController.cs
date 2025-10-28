using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
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
            return await _context.Billings
                .Include(b => b.Consumer)
                .Include(b => b.Meter)
                .Include(b => b.Arrears)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Billing>> GetBilling(long id)
        {
            var billing = await _context.Billings
                .Include(b => b.Consumer)
                .Include(b => b.Meter)
                .Include(b => b.Arrears)
                .FirstOrDefaultAsync(b => b.BillId == id);

            if (billing == null) return NotFound();
            return billing;
        }

        [HttpPost]
        public async Task<ActionResult<Billing>> PostBilling(Billing billing)
        {
            _context.Billings.Add(billing);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBilling), new { id = billing.BillId }, billing);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBilling(long id, Billing billing)
        {
            if (id != billing.BillId) return BadRequest();
            _context.Entry(billing).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillingExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilling(long id)
        {
            var billing = await _context.Billings.FindAsync(id);
            if (billing == null) return NotFound();
            _context.Billings.Remove(billing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool BillingExists(long id) => _context.Billings.Any(e => e.BillId == id);
    }
}