using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
using SmartMeter.Models;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AddressController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            return await _context.Addresses
                .Include(a => a.Consumer)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddress(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var address = await _context.Addresses
                .Include(a => a.Consumer)
                .FirstOrDefaultAsync(a => a.AddressId == id);

            if (address == null) return NotFound();
            return address;
        }

        [HttpPost]
        public async Task<ActionResult<Address>> PostAddress(AddressDto addressDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var address = new Address
            {
                HouseNumber = addressDto.HouseNumber,
                Locality = addressDto.Locality,
                City = addressDto.City,
                State = addressDto.State,
                Pincode = addressDto.Pincode,
                ConsumerId = addressDto.ConsumerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAddress), new { id = address.AddressId }, address);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAddress(long id, AddressDto addressDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            if (addressDto.AddressId.HasValue && id != addressDto.AddressId.Value)
                return BadRequest("ID mismatch");

            var existingAddress = await _context.Addresses.FindAsync(id);
            if (existingAddress == null)
                return NotFound();

            // Update fields
            existingAddress.HouseNumber = addressDto.HouseNumber;
            existingAddress.Locality = addressDto.Locality;
            existingAddress.City = addressDto.City;
            existingAddress.State = addressDto.State;
            existingAddress.Pincode = addressDto.Pincode;
            existingAddress.ConsumerId = addressDto.ConsumerId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AddressExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var address = await _context.Addresses.FindAsync(id);
            if (address == null) return NotFound();
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool AddressExists(long id) => _context.Addresses.Any(e => e.AddressId == id);
    }
}