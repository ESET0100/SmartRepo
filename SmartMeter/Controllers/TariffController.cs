using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.Models;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TariffController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TariffController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "User,Consumer")]
        public async Task<ActionResult<IEnumerable<Tariff>>> GetTariffs()
        {
            return await _context.Tariffs
                .Include(t => t.TodRules)
                .Include(t => t.TariffSlabs)
                .Include(t => t.Consumers)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User,Consumer")]
        public async Task<ActionResult<Tariff>> GetTariff(int id)
        {
            var tariff = await _context.Tariffs
                .Include(t => t.TodRules)
                .Include(t => t.TariffSlabs)
                .Include(t => t.Consumers)
                .FirstOrDefaultAsync(t => t.TariffId == id);

            if (tariff == null) return NotFound();
            return tariff;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<Tariff>> PostTariff(Tariff tariff)
        {
            _context.Tariffs.Add(tariff);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTariff), new { id = tariff.TariffId }, tariff);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PutTariff(int id, Tariff tariff)
        {
            if (id != tariff.TariffId) return BadRequest();
            _context.Entry(tariff).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!TariffExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteTariff(int id)
        {
            var tariff = await _context.Tariffs.FindAsync(id);
            if (tariff == null) return NotFound();
            _context.Tariffs.Remove(tariff);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool TariffExists(int id) => _context.Tariffs.Any(e => e.TariffId == id);
    }
}