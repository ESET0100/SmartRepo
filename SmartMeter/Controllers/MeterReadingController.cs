using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
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
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            return await _context.MeterReadings
                .Include(m => m.Meter)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MeterReading>> GetMeterReading(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var meterReading = await _context.MeterReadings
                .Include(m => m.Meter)
                .FirstOrDefaultAsync(m => m.ReadingId == id);

            if (meterReading == null) return NotFound();
            return meterReading;
        }

        [HttpPost]
        public async Task<ActionResult<MeterReading>> PostMeterReading(MeterReadingDto meterReadingDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var meterReading = new MeterReading
            {
                ReadingDate = meterReadingDto.ReadingDate,
                EnergyConsumed = meterReadingDto.EnergyConsumed,
                MeterSerialNo = meterReadingDto.MeterSerialNo,
                Current = meterReadingDto.Current,
                Voltage = meterReadingDto.Voltage
            };

            _context.MeterReadings.Add(meterReading);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeterReading), new { id = meterReading.ReadingId }, meterReading);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMeterReading(long id, MeterReadingDto meterReadingDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            if (meterReadingDto.ReadingId.HasValue && id != meterReadingDto.ReadingId.Value)
                return BadRequest("ID mismatch");

            var existingMeterReading = await _context.MeterReadings.FindAsync(id);
            if (existingMeterReading == null)
                return NotFound();

            // Update fields
            existingMeterReading.ReadingDate = meterReadingDto.ReadingDate;
            existingMeterReading.EnergyConsumed = meterReadingDto.EnergyConsumed;
            existingMeterReading.MeterSerialNo = meterReadingDto.MeterSerialNo;
            existingMeterReading.Current = meterReadingDto.Current;
            existingMeterReading.Voltage = meterReadingDto.Voltage;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeterReadingExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMeterReading(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var meterReading = await _context.MeterReadings.FindAsync(id);
            if (meterReading == null) return NotFound();
            _context.MeterReadings.Remove(meterReading);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool MeterReadingExists(long id) => _context.MeterReadings.Any(e => e.ReadingId == id);
    }
}