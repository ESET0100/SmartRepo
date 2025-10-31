using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.Models;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrgUnitController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrgUnitController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrgUnit>>> GetOrgUnits()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            return await _context.OrgUnits
                .Include(o => o.Parent)
                .Include(o => o.Children)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrgUnit>> GetOrgUnit(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var orgUnit = await _context.OrgUnits
                .Include(o => o.Parent)
                .Include(o => o.Children)
                .FirstOrDefaultAsync(o => o.OrgUnitId == id);

            if (orgUnit == null) return NotFound();
            return orgUnit;
        }

        [HttpPost]
        public async Task<ActionResult<OrgUnit>> PostOrgUnit(OrgUnit orgUnit)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            _context.OrgUnits.Add(orgUnit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrgUnit), new { id = orgUnit.OrgUnitId }, orgUnit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrgUnit(int id, OrgUnit orgUnit)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            if (id != orgUnit.OrgUnitId) return BadRequest();
            _context.Entry(orgUnit).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrgUnitExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrgUnit(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }
            var orgUnit = await _context.OrgUnits.FindAsync(id);
            if (orgUnit == null) return NotFound();
            _context.OrgUnits.Remove(orgUnit);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool OrgUnitExists(int id) => _context.OrgUnits.Any(e => e.OrgUnitId == id);
    }
}