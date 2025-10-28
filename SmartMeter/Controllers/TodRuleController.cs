using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.Models;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodRuleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TodRuleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodRule>>> GetTodRules()
        {
            return await _context.TodRules
                .Include(t => t.Tariff)
                .Where(t => !t.Deleted)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodRule>> GetTodRule(int id)
        {
            var todRule = await _context.TodRules
                .Include(t => t.Tariff)
                .FirstOrDefaultAsync(t => t.TodRuleId == id && !t.Deleted);

            if (todRule == null) return NotFound();
            return todRule;
        }

        [HttpPost]
        public async Task<ActionResult<TodRule>> PostTodRule(TodRule todRule)
        {
            _context.TodRules.Add(todRule);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTodRule), new { id = todRule.TodRuleId }, todRule);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodRule(int id, TodRule todRule)
        {
            if (id != todRule.TodRuleId) return BadRequest();
            _context.Entry(todRule).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodRuleExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodRule(int id)
        {
            var todRule = await _context.TodRules.FindAsync(id);
            if (todRule == null) return NotFound();

            // Soft delete
            todRule.Deleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool TodRuleExists(int id) => _context.TodRules.Any(e => e.TodRuleId == id && !e.Deleted);
    }
}