using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.Models;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArrearsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArrearsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Arrears>>> GetArrears()
        {
            return await _context.Arrears
                .Include(a => a.Consumer)
                .Include(a => a.Billing)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Arrears>> GetArrears(long id)
        {
            var arrears = await _context.Arrears
                .Include(a => a.Consumer)
                .Include(a => a.Billing)
                .FirstOrDefaultAsync(a => a.ArrearId == id);

            if (arrears == null) return NotFound();
            return arrears;
        }

        [HttpPost]
        public async Task<ActionResult<Arrears>> PostArrears(Arrears arrears)
        {
            _context.Arrears.Add(arrears);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetArrears), new { id = arrears.ArrearId }, arrears);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutArrears(long id, Arrears arrears)
        {
            if (id != arrears.ArrearId) return BadRequest();
            _context.Entry(arrears).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArrearsExists(id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArrears(long id)
        {
            var arrears = await _context.Arrears.FindAsync(id);
            if (arrears == null) return NotFound();
            _context.Arrears.Remove(arrears);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ArrearsExists(long id) => _context.Arrears.Any(e => e.ArrearId == id);
    }
}