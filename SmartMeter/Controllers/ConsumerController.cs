using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
using SmartMeter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ConsumerController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Consumer>>> GetConsumers()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            return await _context.Consumers
                .Include(c => c.OrgUnit)
                .Include(c => c.Tariff)
                .Include(c => c.Addresses)
                .Where(c => !c.Deleted)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Consumer>> GetConsumer(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var consumer = await _context.Consumers
                .Include(c => c.OrgUnit)
                .Include(c => c.Tariff)
                .Include(c => c.Addresses)
                .Include(c => c.Meters)
                .FirstOrDefaultAsync(c => c.ConsumerId == id && !c.Deleted);

            if (consumer == null)
            {
                return NotFound();
            }

            return consumer;
        }

        [HttpPost]
        public async Task<ActionResult<Consumer>> PostConsumer(ConsumerDto consumerDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var consumer = new Consumer
            {
                Name = consumerDto.Name,
                Phone = consumerDto.Phone,
                Email = consumerDto.Email,
                OrgUnitId = consumerDto.OrgUnitId,
                TariffId = consumerDto.TariffId,
                Status = consumerDto.Status,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "system"
            };

            _context.Consumers.Add(consumer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConsumer), new { id = consumer.ConsumerId }, consumer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutConsumer(long id, ConsumerDto consumerDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            if (consumerDto.ConsumerId.HasValue && id != consumerDto.ConsumerId.Value)
                return BadRequest("ID mismatch");

            var existingConsumer = await _context.Consumers.FindAsync(id);
            if (existingConsumer == null)
                return NotFound();

            // Update fields
            existingConsumer.Name = consumerDto.Name;
            existingConsumer.Phone = consumerDto.Phone;
            existingConsumer.Email = consumerDto.Email;
            existingConsumer.OrgUnitId = consumerDto.OrgUnitId;
            existingConsumer.TariffId = consumerDto.TariffId;
            existingConsumer.Status = consumerDto.Status;
            existingConsumer.UpdatedAt = DateTime.UtcNow;
            existingConsumer.UpdatedBy = User.Identity?.Name ?? "system";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsumerExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumer(long id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var consumer = await _context.Consumers.FindAsync(id);
            if (consumer == null)
            {
                return NotFound();
            }

            // Soft delete
            consumer.Deleted = true;
            consumer.UpdatedAt = DateTime.UtcNow;
            consumer.UpdatedBy = User.Identity?.Name ?? "system";

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConsumerExists(long id)
        {
            return _context.Consumers.Any(e => e.ConsumerId == id && !e.Deleted);
        }

        [HttpPost("{id}/photo")]
        public async Task<IActionResult> UploadPhoto(long id, IFormFile photo)
        {
            var consumer = await _context.Consumers.FirstOrDefaultAsync(c => c.ConsumerId == id && !c.Deleted);
            if (consumer == null) return NotFound();

            if (photo == null || photo.Length == 0)
                return BadRequest("Photo file is required");

            if (!photo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only image files are allowed");

            var uploadsRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var consumerUploads = Path.Combine(uploadsRoot, "uploads", "consumers");
            Directory.CreateDirectory(consumerUploads);

            var extension = Path.GetExtension(photo.FileName);
            var safeExtension = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension;
            var fileName = $"{id}_{Guid.NewGuid():N}{safeExtension}";
            var filePath = Path.Combine(consumerUploads, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await photo.CopyToAsync(stream);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var publicUrl = $"{baseUrl}/uploads/consumers/{fileName}";

            consumer.PhotoUrl = publicUrl;
            consumer.UpdatedAt = DateTime.UtcNow;
            consumer.UpdatedBy = User.Identity?.Name ?? "system";
            await _context.SaveChangesAsync();

            return Ok(new { photoUrl = publicUrl });
        }
    }
}