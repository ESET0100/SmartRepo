using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
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
            return await _context.TariffSlabs
                .Include(t => t.Tariff)
                .Where(t => !t.Deleted)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TariffSlab>> GetTariffSlab(int id)
        {
            var tariffSlab = await _context.TariffSlabs
                .Include(t => t.Tariff)
                .FirstOrDefaultAsync(t => t.TariffSlabId == id && !t.Deleted);

            if (tariffSlab == null) return NotFound();
            return tariffSlab;
        }

        [HttpPost]
        public async Task<ActionResult<TariffSlab>> PostTariffSlab(TariffSlab tariffSlab)
        {
            _context.TariffSlabs.Add(tariffSlab);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTariffSlab), new { id = tariffSlab.TariffSlabId }, tariffSlab);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTariffSlab(int id, TariffSlab tariffSlab)
        {
            if (id != tariffSlab.TariffSlabId) return BadRequest();
            _context.Entry(tariffSlab).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!TariffSlabExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTariffSlab(int id)
        {
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