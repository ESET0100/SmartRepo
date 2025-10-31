using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
using SmartMeter.Models;

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
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<Meter>>> GetMeters()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            return await _context.Meters
                .Include(m => m.Consumer)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<Meter>> GetMeter(string id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

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
        [Authorize(Roles = "User")]
        public async Task<ActionResult<Meter>> PostMeter(MeterDto meterDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var meter = new Meter
            {
                MeterSerialNo = meterDto.MeterSerialNo,
                IpAddress = meterDto.IpAddress,
                ICCID = meterDto.ICCID,
                IMSI = meterDto.IMSI,
                Manufacturer = meterDto.Manufacturer,
                Firmware = meterDto.Firmware,
                Category = meterDto.Category,
                InstallTsUtc = meterDto.InstallTsUtc,
                Status = meterDto.Status,
                ConsumerId = meterDto.ConsumerId
            };

            _context.Meters.Add(meter);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeter), new { id = meter.MeterSerialNo }, meter);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PutMeter(string id, MeterDto meterDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            // For Meter, ID is string (MeterSerialNo)
            if (!string.IsNullOrEmpty(meterDto.MeterSerialNo) && id != meterDto.MeterSerialNo)
                return BadRequest("MeterSerialNo mismatch");

            var existingMeter = await _context.Meters.FindAsync(id);
            if (existingMeter == null)
                return NotFound();

            // Update fields
            existingMeter.IpAddress = meterDto.IpAddress;
            existingMeter.ICCID = meterDto.ICCID;
            existingMeter.IMSI = meterDto.IMSI;
            existingMeter.Manufacturer = meterDto.Manufacturer;
            existingMeter.Firmware = meterDto.Firmware;
            existingMeter.Category = meterDto.Category;
            existingMeter.InstallTsUtc = meterDto.InstallTsUtc;
            existingMeter.Status = meterDto.Status;
            existingMeter.ConsumerId = meterDto.ConsumerId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MeterExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteMeter(string id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

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