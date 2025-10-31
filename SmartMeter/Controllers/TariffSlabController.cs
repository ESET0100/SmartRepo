using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
using SmartMeter.Models;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TariffSlabController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TariffSlabController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TariffSlab>>> GetTariffSlabs()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            return await _context.TariffSlabs
                .Include(t => t.Tariff)
                .Where(t => !t.Deleted)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TariffSlab>> GetTariffSlab(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }
            var tariffSlab = await _context.TariffSlabs
                .Include(t => t.Tariff)
                .FirstOrDefaultAsync(t => t.TariffSlabId == id && !t.Deleted);

            if (tariffSlab == null) return NotFound();
            return tariffSlab;
        }

        [HttpPost]
        public async Task<ActionResult<TariffSlab>> PostTariffSlab(TariffSlabDto tariffSlabDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }
            var tariffSlab = new TariffSlab
            {
                TariffId = tariffSlabDto.TariffId,
                FromKwh = tariffSlabDto.FromKwh,
                ToKwh = tariffSlabDto.ToKwh,
                RatePerKwh = tariffSlabDto.RatePerKwh
            };

            _context.TariffSlabs.Add(tariffSlab);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTariffSlab), new { id = tariffSlab.TariffSlabId }, tariffSlab);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTariffSlab(int id, TariffSlabDto tariffSlabDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            if (tariffSlabDto.TariffSlabId.HasValue && id != tariffSlabDto.TariffSlabId.Value)
                return BadRequest("ID mismatch");

            var existingTariffSlab = await _context.TariffSlabs.FindAsync(id);
            if (existingTariffSlab == null)
                return NotFound();

            // Update fields
            existingTariffSlab.TariffId = tariffSlabDto.TariffId;
            existingTariffSlab.FromKwh = tariffSlabDto.FromKwh;
            existingTariffSlab.ToKwh = tariffSlabDto.ToKwh;
            existingTariffSlab.RatePerKwh = tariffSlabDto.RatePerKwh;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TariffSlabExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTariffSlab(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var tariffSlab = await _context.TariffSlabs.FindAsync(id);
            if (tariffSlab == null) return NotFound();

            // Soft delete
            tariffSlab.Deleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool TariffSlabExists(int id) => _context.TariffSlabs.Any(e => e.TariffSlabId == id && !e.Deleted);
    }
}