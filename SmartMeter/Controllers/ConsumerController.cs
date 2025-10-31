using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
using SmartMeter.Models;
using SmartMeter.Services;
using System.IO;
using System.Security.Claims;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IAuthService _authService;

        public ConsumerController(ApplicationDbContext context, IWebHostEnvironment env, IAuthService authService)
        {
            _context = context;
            _env = env;
            _authService = authService;
        }

        // ONLY USERS CAN GET ALL CONSUMERS
        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<Consumer>>> GetConsumers()
        {
            return await _context.Consumers
                .Include(c => c.OrgUnit)
                .Include(c => c.Tariff)
                .Include(c => c.Addresses)
                .Where(c => !c.Deleted)
                .ToListAsync();
        }

        // USERS CAN GET ANY CONSUMER, CONSUMERS CAN ONLY GET THEMSELVES
        [HttpGet("{id}")]
        [Authorize(Roles = "User,Consumer")]
        public async Task<ActionResult<Consumer>> GetConsumer(long id)
        {
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

            // Check if consumer is trying to access another consumer's data
            if (User.IsInRole("Consumer"))
            {
                var consumerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(consumerIdClaim) || !long.TryParse(consumerIdClaim, out var loggedInConsumerId) || id != loggedInConsumerId)
                {
                    return Forbid("You can only access your own data");
                }
            }

            return consumer;
        }

        // ONLY USERS CAN CREATE CONSUMERS
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<Consumer>> PostConsumer(CreateConsumerDto createConsumerDto)
        {
            // Check if consumer with email already exists
            if (await _authService.ConsumerExistsAsync(createConsumerDto.Email))
            {
                return BadRequest("Consumer with this email already exists");
            }
            //var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
            var username = User.FindFirst("username")?.Value ?? "Lakshay";

            var consumer = new Consumer
            {
                Name = createConsumerDto.Name,
                Phone = createConsumerDto.Phone,
                Email = createConsumerDto.Email, // This is not null due to validation
                OrgUnitId = createConsumerDto.OrgUnitId,
                TariffId = createConsumerDto.TariffId,
                Status = createConsumerDto.Status,
                CreatedAt = DateTime.UtcNow,
                // CreatedBy = User.Identity?.Name ?? "system"
                CreatedBy = username
            };

            var result = await _authService.CreateConsumerWithPasswordAsync(consumer, createConsumerDto.Password);
            if (result == null)
                return BadRequest("Failed to create consumer");

            return CreatedAtAction(nameof(GetConsumer), new { id = consumer.ConsumerId }, new ConsumerDto
            {
                ConsumerId = consumer.ConsumerId,
                Name = consumer.Name,
                Phone = consumer.Phone,
                Email = consumer.Email,
                OrgUnitId = consumer.OrgUnitId,
                TariffId = consumer.TariffId,
                Status = consumer.Status
            });
        }

        // ONLY USERS CAN UPDATE ANY CONSUMER
        [HttpPut("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PutConsumer(long id, ConsumerDto consumerDto)
        {
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

        // CONSUMER SELF-UPDATE ENDPOINT
        [HttpPut("profile")]
        [Authorize(Roles = "Consumer")]
        public async Task<IActionResult> UpdateConsumerProfile(ConsumerUpdateDto consumerUpdateDto)
        {
            var consumerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(consumerIdClaim) || !long.TryParse(consumerIdClaim, out var consumerId))
                return Unauthorized();

            var existingConsumer = await _context.Consumers.FindAsync(consumerId);
            if (existingConsumer == null)
                return NotFound();

            // Consumers can only update their name, phone, and email
            existingConsumer.Name = consumerUpdateDto.Name;
            existingConsumer.Phone = consumerUpdateDto.Phone;
            existingConsumer.Email = consumerUpdateDto.Email;
            existingConsumer.UpdatedAt = DateTime.UtcNow;
            existingConsumer.UpdatedBy = "consumer-self";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsumerExists(consumerId))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ONLY USERS CAN DELETE/SOFT-DELETE CONSUMERS
        [HttpDelete("{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> DeleteConsumer(long id)
        {
            var consumer = await _context.Consumers.FindAsync(id);
            if (consumer == null)
            {
                return NotFound();
            }

            // Soft delete
            consumer.Deleted = true;
            consumer.Status = "Inactive";
            consumer.UpdatedAt = DateTime.UtcNow;
            consumer.UpdatedBy = User.Identity?.Name ?? "system";

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // CONSUMER GET OWN PROFILE
        [HttpGet("profile")]
        [Authorize(Roles = "Consumer")]
        public async Task<ActionResult<Consumer>> GetConsumerProfile()
        {
            var consumerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(consumerIdClaim) || !long.TryParse(consumerIdClaim, out var consumerId))
                return Unauthorized();

            var consumer = await _context.Consumers
                .Include(c => c.OrgUnit)
                .Include(c => c.Tariff)
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.ConsumerId == consumerId && !c.Deleted);

            if (consumer == null)
                return NotFound();

            return consumer;
        }

        private bool ConsumerExists(long id)
        {
            return _context.Consumers.Any(e => e.ConsumerId == id && !e.Deleted);
        }

        // Existing photo upload method...
        [HttpPost("{id}/photo")]
        [Authorize(Roles = "User,Consumer")]
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