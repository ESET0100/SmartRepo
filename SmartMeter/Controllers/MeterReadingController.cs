using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.Models;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeterReadingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MeterReadingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeterReading>>> GetMeterReadings()
        {
            return await _context.MeterReadings
                .Include(m => m.Meter)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MeterReading>> GetMeterReading(long id)
        {
            var meterReading = await _context.MeterReadings
                .Include(m => m.Meter)
                .FirstOrDefaultAsync(m => m.ReadingId == id);

            if (meterReading == null) return NotFound();
            return meterReading;
        }

        [HttpPost]
        public async Task<ActionResult<MeterReading>> PostMeterReading(MeterReading meterReading)
        {
            _context.MeterReadings.Add(meterReading);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMeterReading), new { id = meterReading.ReadingId }, meterReading);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeterReading(long id, MeterReading meterReading)
        {
            if (id != meterReading.ReadingId) return BadRequest();
            _context.Entry(meterReading).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeterReadingExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeterReading(long id)
        {
            var meterReading = await _context.MeterReadings.FindAsync(id);
            if (meterReading == null) return NotFound();
            _context.MeterReadings.Remove(meterReading);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool MeterReadingExists(long id) => _context.MeterReadings.Any(e => e.ReadingId == id);
    }
}