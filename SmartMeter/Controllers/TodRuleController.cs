using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
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
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            return await _context.TodRules
                .Include(t => t.Tariff)
                .Where(t => !t.Deleted)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodRule>> GetTodRule(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var todRule = await _context.TodRules
                .Include(t => t.Tariff)
                .FirstOrDefaultAsync(t => t.TodRuleId == id && !t.Deleted);

            if (todRule == null) return NotFound();
            return todRule;
        }

        [HttpPost]
        public async Task<ActionResult<TodRule>> PostTodRule(TodRuleDto todRuleDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var todRule = new TodRule
            {
                TariffId = todRuleDto.TariffId,
                Name = todRuleDto.Name,
                StartTime = todRuleDto.StartTime,
                EndTime = todRuleDto.EndTime,
                RatePerKwh = todRuleDto.RatePerKwh
            };

            _context.TodRules.Add(todRule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodRule), new { id = todRule.TodRuleId }, todRule);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodRule(int id, TodRuleDto todRuleDto)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            // For PUT, we can use the ID from route OR from DTO - both should match
            if (todRuleDto.TodRuleId.HasValue && id != todRuleDto.TodRuleId.Value)
                return BadRequest("ID mismatch");

            var existingTodRule = await _context.TodRules.FindAsync(id);
            if (existingTodRule == null)
                return NotFound();

            // Update fields
            existingTodRule.TariffId = todRuleDto.TariffId;
            existingTodRule.Name = todRuleDto.Name;
            existingTodRule.StartTime = todRuleDto.StartTime;
            existingTodRule.EndTime = todRuleDto.EndTime;
            existingTodRule.RatePerKwh = todRuleDto.RatePerKwh;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodRuleExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodRule(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }
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