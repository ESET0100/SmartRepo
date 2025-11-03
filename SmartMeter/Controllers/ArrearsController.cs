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
    public class ArrearsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArrearsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<Arrears>>> GetArrears()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }
            return await _context.Arrears
                .Include(a => a.Consumer)
                .Include(a => a.Billing)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<Arrears>> GetArrears(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var arrears = await _context.Arrears
                .Include(a => a.Consumer)
                .Include(a => a.Billing)
                .FirstOrDefaultAsync(a => a.ArrearId == id);

            if (arrears == null) return NotFound();
            return arrears;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<Arrears>> PostArrears(ArrearsDto arrearsDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var arrears = new Arrears
            {
                ConsumerId = arrearsDto.ConsumerId,
                ArrearType = arrearsDto.ArrearType,
                PaidStatus = arrearsDto.PaidStatus,
                BillId = arrearsDto.BillId,
                ArrearAmount = arrearsDto.ArrearAmount,
                CreatedAt = DateTime.UtcNow
            };

            _context.Arrears.Add(arrears);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArrears), new { id = arrears.ArrearId }, arrears);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PutArrears(long id, ArrearsDto arrearsDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            if (arrearsDto.ArrearId.HasValue && id != arrearsDto.ArrearId.Value)
                return BadRequest("ID mismatch");

            var existingArrears = await _context.Arrears.FindAsync(id);
            if (existingArrears == null)
                return NotFound();

            // Update fields
            existingArrears.ConsumerId = arrearsDto.ConsumerId;
            existingArrears.ArrearType = arrearsDto.ArrearType;
            existingArrears.PaidStatus = arrearsDto.PaidStatus;
            existingArrears.BillId = arrearsDto.BillId;
            existingArrears.ArrearAmount = arrearsDto.ArrearAmount;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArrearsExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteArrears(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var arrears = await _context.Arrears.FindAsync(id);
            if (arrears == null) return NotFound();
            _context.Arrears.Remove(arrears);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ArrearsExists(long id) => _context.Arrears.Any(e => e.ArrearId == id);
    }
}