using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.Models;
using SmartMeter.Data;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MeterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Meter>>> GetMeters()
        {
            return await _context.Meters
                .Include(m => m.Consumer)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Meter>> GetMeter(string id)
        {
            var meter = await _context.Meters
                .Include(m => m.Consumer)
                .FirstOrDefaultAsync(m => m.MeterSerialNo == id);

            if (meter == null)
            {
                return NotFound();
            }

            return meter;
        }

        [HttpPost]
        public async Task<ActionResult<Meter>> PostMeter(Meter meter)
        {
            _context.Meters.Add(meter);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeter), new { id = meter.MeterSerialNo }, meter);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeter(string id, Meter meter)
        {
            if (id != meter.MeterSerialNo)
            {
                return BadRequest();
            }

            _context.Entry(meter).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeterExists(id))
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
        public async Task<IActionResult> DeleteMeter(string id)
        {
            var meter = await _context.Meters.FindAsync(id);
            if (meter == null)
            {
                return NotFound();
            }

            _context.Meters.Remove(meter);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MeterExists(string id)
        {
            return _context.Meters.Any(e => e.MeterSerialNo == id);
        }
    }
}