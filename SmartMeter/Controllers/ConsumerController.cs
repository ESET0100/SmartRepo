using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.Models;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConsumerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Consumer>>> GetConsumers()
        {
            return await _context.Consumers
                .Include(c => c.OrgUnit)
                .Include(c => c.Tariff)
                .Include(c => c.Addresses)
                .Where(c => !c.Deleted)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Consumer>> GetConsumer(long id)
        {
            var consumer = await _context.Consumers
                .Include(c => c.OrgUnit)
                .Include(c => c.Tariff)
                .Include(c => c.Addresses)
                .Include(c => c.Meters)
                .FirstOrDefaultAsync(c => c.ConsumerId == id && !c.Deleted);

            if (consumer == null)
            {
                return NotFound();
            }

            return consumer;
        }

        [HttpPost]
        public async Task<ActionResult<Consumer>> PostConsumer(Consumer consumer)
        {
            consumer.CreatedAt = DateTime.UtcNow;
            consumer.CreatedBy = User.Identity?.Name ?? "system";

            _context.Consumers.Add(consumer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConsumer), new { id = consumer.ConsumerId }, consumer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutConsumer(long id, Consumer consumer)
        {
            if (id != consumer.ConsumerId)
            {
                return BadRequest();
            }

            consumer.UpdatedAt = DateTime.UtcNow;
            consumer.UpdatedBy = User.Identity?.Name ?? "system";

            _context.Entry(consumer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsumerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumer(long id)
        {
            var consumer = await _context.Consumers.FindAsync(id);
            if (consumer == null)
            {
                return NotFound();
            }

            // Soft delete
            consumer.Deleted = true;
            consumer.UpdatedAt = DateTime.UtcNow;
            consumer.UpdatedBy = User.Identity?.Name ?? "system";

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConsumerExists(long id)
        {
            return _context.Consumers.Any(e => e.ConsumerId == id && !e.Deleted);
        }
    }
}